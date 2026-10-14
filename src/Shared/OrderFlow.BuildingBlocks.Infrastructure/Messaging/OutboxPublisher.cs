using System.Diagnostics;
using System.Text;
using Dapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using OrderFlow.BuildingBlocks.ServiceDefaults.Telemetry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace OrderFlow.BuildingBlocks.Infrastructure.Messaging;

internal sealed partial class OutboxPublisher(
    NpgsqlDataSource dataSource,
    RabbitMqConnection rabbitMq,
    IOptions<OutboxOptions> options,
    TimeProvider timeProvider,
    ILogger<OutboxPublisher> logger
) : BackgroundService
{
    private const string SelectSql = """
        SELECT id AS Id, type AS Type, payload::text AS Payload, occurred_at AS OccurredAt,
               correlation_id AS CorrelationId, trace_parent AS TraceParent
        FROM outbox_messages
        WHERE processed_at IS NULL
        ORDER BY occurred_at, id
        LIMIT @BatchSize
        FOR UPDATE SKIP LOCKED;
        """;

    private const string ProcessedSql =
        "UPDATE outbox_messages SET processed_at = @Now, attempts = attempts + 1, last_error = NULL WHERE id = @Id;";

    private const string FailedSql =
        "UPDATE outbox_messages SET attempts = attempts + 1, last_error = @Error WHERE id = @Id;";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMilliseconds(options.Value.PollIntervalMilliseconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            var published = 0;
            try
            {
                published = await PublishBatchAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
                when (exception is NpgsqlException or RabbitMQClientException or IOException)
            {
                LogBatchFailed(exception);
            }

            if (published == 0)
            {
                await Task.Delay(interval, timeProvider, stoppingToken)
                    .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
        }
    }

    private async Task<int> PublishBatchAsync(CancellationToken cancellationToken)
    {
        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var transaction = await connection
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);
            await using (transaction.ConfigureAwait(false))
            {
                var messages = (
                    await connection
                        .QueryAsync<OutboxMessage>(
                            new CommandDefinition(
                                SelectSql,
                                new { options.Value.BatchSize },
                                transaction,
                                cancellationToken: cancellationToken
                            )
                        )
                        .ConfigureAwait(false)
                ).ToList();

                if (messages.Count == 0)
                {
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                    return 0;
                }

                var channel = await CreateChannelAsync(cancellationToken).ConfigureAwait(false);
                await using (channel.ConfigureAwait(false))
                {
                    foreach (var message in messages)
                    {
                        var error = await PublishAsync(channel, message, cancellationToken)
                            .ConfigureAwait(false);
                        await connection
                            .ExecuteAsync(
                                new CommandDefinition(
                                    error is null ? ProcessedSql : FailedSql,
                                    new
                                    {
                                        message.Id,
                                        Now = timeProvider.GetUtcNow(),
                                        Error = error,
                                    },
                                    transaction,
                                    cancellationToken: cancellationToken
                                )
                            )
                            .ConfigureAwait(false);
                    }
                }

                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return messages.Count;
            }
        }
    }

    private async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken)
    {
        var connection = await rabbitMq.GetAsync(cancellationToken).ConfigureAwait(false);
        var channel = await connection
            .CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true
                ),
                cancellationToken
            )
            .ConfigureAwait(false);
        await channel
            .ExchangeDeclareAsync(
                rabbitMq.Options.Exchange,
                ExchangeType.Topic,
                durable: true,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);
        return channel;
    }

    private async Task<string?> PublishAsync(
        IChannel channel,
        OutboxMessage message,
        CancellationToken cancellationToken
    )
    {
        ActivityContext.TryParse(message.TraceParent, null, out var parent);
        using var activity = MessagingTelemetry.Source.StartActivity(
            $"{message.Type} publish",
            ActivityKind.Producer,
            parent
        );
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination.name", rabbitMq.Options.Exchange);
        activity?.SetTag("messaging.message.id", message.Id.ToString());

        var properties = new BasicProperties
        {
            MessageId = message.Id.ToString(),
            Type = message.Type,
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                [MessageHeaders.CorrelationId] = message.CorrelationId,
                [MessageHeaders.TraceParent] = activity?.Id ?? message.TraceParent,
                [MessageHeaders.OccurredAt] = message.OccurredAt.ToString("O"),
            },
        };

        try
        {
            await channel
                .BasicPublishAsync(
                    rabbitMq.Options.Exchange,
                    message.Type,
                    mandatory: true,
                    properties,
                    Encoding.UTF8.GetBytes(message.Payload),
                    cancellationToken
                )
                .ConfigureAwait(false);
            LogPublished(message.Type, message.Id);
            return null;
        }
        catch (PublishException exception)
        {
            LogPublishRejected(exception, message.Type, message.Id);
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            return exception.IsReturn
                ? "Unroutable: no queue is bound for this message type yet."
                : "Rejected by the broker.";
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Published {MessageType} {MessageId}")]
    private partial void LogPublished(string messageType, Guid messageId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Broker did not accept {MessageType} {MessageId}; it stays in the outbox"
    )]
    private partial void LogPublishRejected(
        Exception exception,
        string messageType,
        Guid messageId
    );

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Outbox batch failed; retrying on the next poll"
    )]
    private partial void LogBatchFailed(Exception exception);
}
