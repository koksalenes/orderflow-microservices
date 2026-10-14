using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.ServiceDefaults.Correlation;
using OrderFlow.BuildingBlocks.ServiceDefaults.Telemetry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace OrderFlow.BuildingBlocks.Infrastructure.Messaging;

internal sealed record ConsumerRegistration(string MessageType, string Queue);

internal static class ConsumerDefaults
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);
}

internal sealed partial class RabbitMqConsumer<TMessage>(
    ConsumerRegistration registration,
    RabbitMqConnection rabbitMq,
    NpgsqlDataSource dataSource,
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<RabbitMqConsumer<TMessage>> logger
) : BackgroundService
    where TMessage : class
{
    private const int MaxRetries = 5;
    private const ushort Prefetch = 10;

    private const string InboxExistsSql =
        "SELECT EXISTS (SELECT 1 FROM inbox_messages WHERE message_id = @MessageId AND consumer = @Consumer);";

    private const string InboxInsertSql = """
        INSERT INTO inbox_messages (message_id, consumer, message_type, processed_at)
        VALUES (@MessageId, @Consumer, @MessageType, @ProcessedAt)
        ON CONFLICT DO NOTHING;
        """;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var channel = await StartConsumingAsync(stoppingToken).ConfigureAwait(false);
                await using (channel.ConfigureAwait(false))
                {
                    LogConsuming(registration.Queue, registration.MessageType);
                    while (channel.IsOpen && !stoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(
                                ConsumerDefaults.ReconnectDelay,
                                timeProvider,
                                stoppingToken
                            )
                            .ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception) when (exception is RabbitMQClientException or IOException)
            {
                LogConnectFailed(exception, registration.Queue);
                await Task.Delay(ConsumerDefaults.ReconnectDelay, timeProvider, stoppingToken)
                    .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
        }
    }

    private async Task<IChannel> StartConsumingAsync(CancellationToken cancellationToken)
    {
        var settings = rabbitMq.Options;
        var queue = registration.Queue;
        var connection = await rabbitMq.GetAsync(cancellationToken).ConfigureAwait(false);
        var channel = await connection
            .CreateChannelAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await channel
            .ExchangeDeclareAsync(
                settings.Exchange,
                ExchangeType.Topic,
                durable: true,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);
        await channel
            .ExchangeDeclareAsync(
                settings.DeadLetterExchange,
                ExchangeType.Direct,
                durable: true,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);
        await channel
            .QueueDeclareAsync(
                $"{queue}.dlq",
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);
        await channel
            .QueueBindAsync(
                $"{queue}.dlq",
                settings.DeadLetterExchange,
                queue,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);
        await channel
            .QueueDeclareAsync(
                queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["x-dead-letter-exchange"] = settings.DeadLetterExchange,
                    ["x-dead-letter-routing-key"] = queue,
                },
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);
        await channel
            .QueueBindAsync(
                queue,
                settings.Exchange,
                registration.MessageType,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);
        await channel
            .BasicQosAsync(0, Prefetch, global: false, cancellationToken)
            .ConfigureAwait(false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, delivery) =>
            OnReceivedAsync(channel, delivery, cancellationToken);
        await channel
            .BasicConsumeAsync(queue, autoAck: false, consumer, cancellationToken)
            .ConfigureAwait(false);

        return channel;
    }

    private async Task OnReceivedAsync(
        IChannel channel,
        BasicDeliverEventArgs delivery,
        CancellationToken cancellationToken
    )
    {
        var properties = delivery.BasicProperties;
        var correlationId = HeaderString(properties, MessageHeaders.CorrelationId);
        using var correlation = CorrelationId.IsValid(correlationId)
            ? CorrelationContext.Begin(correlationId!)
            : null;

        ActivityContext.TryParse(
            HeaderString(properties, MessageHeaders.TraceParent),
            null,
            out var parent
        );
        using var activity = MessagingTelemetry.Source.StartActivity(
            $"{registration.MessageType} process",
            ActivityKind.Consumer,
            parent
        );
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination.name", registration.Queue);
        activity?.SetTag("messaging.message.id", properties.MessageId);

        if (!Guid.TryParse(properties.MessageId, out var messageId))
        {
            LogUnreadable(registration.Queue, properties.MessageId);
            await channel
                .BasicRejectAsync(delivery.DeliveryTag, requeue: false, cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        try
        {
            if (await IsInInboxAsync(messageId, cancellationToken).ConfigureAwait(false))
            {
                LogDuplicate(registration.MessageType, messageId);
                await channel
                    .BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            var message =
                JsonSerializer.Deserialize<TMessage>(
                    delivery.Body.Span,
                    ConsumerDefaults.JsonOptions
                ) ?? throw new JsonException("The message body is empty.");
            var context = new MessageContext(
                messageId,
                registration.MessageType,
                DateTimeOffset.TryParse(
                    HeaderString(properties, MessageHeaders.OccurredAt),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var occurredAt
                )
                    ? occurredAt
                    : timeProvider.GetUtcNow(),
                correlationId
            );

            var scope = scopeFactory.CreateAsyncScope();
            await using (scope.ConfigureAwait(false))
            {
                await scope
                    .ServiceProvider.GetRequiredService<IIntegrationMessageHandler<TMessage>>()
                    .HandleAsync(message, context, cancellationToken)
                    .ConfigureAwait(false);
            }

            await AddToInboxAsync(messageId, cancellationToken).ConfigureAwait(false);
            await channel
                .BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken)
                .ConfigureAwait(false);
            LogHandled(registration.MessageType, messageId);
        }
        catch (JsonException exception)
        {
            LogPoison(exception, registration.MessageType, messageId);
            await channel
                .BasicRejectAsync(delivery.DeliveryTag, requeue: false, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            await RetryOrDeadLetterAsync(channel, delivery, messageId, exception, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task RetryOrDeadLetterAsync(
        IChannel channel,
        BasicDeliverEventArgs delivery,
        Guid messageId,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var retries = HeaderInt(delivery.BasicProperties, MessageHeaders.RetryCount);
        if (retries >= MaxRetries)
        {
            LogDeadLettered(exception, registration.MessageType, messageId, retries);
            await channel
                .BasicRejectAsync(delivery.DeliveryTag, requeue: false, cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        var delay = TimeSpan.FromSeconds(Math.Pow(2, retries));
        LogRetrying(
            exception,
            registration.MessageType,
            messageId,
            retries + 1,
            delay.TotalSeconds
        );
        await Task.Delay(delay, timeProvider, cancellationToken).ConfigureAwait(false);

        var properties = new BasicProperties(delivery.BasicProperties)
        {
            Headers = new Dictionary<string, object?>(
                delivery.BasicProperties.Headers
                    ?? new Dictionary<string, object?>(StringComparer.Ordinal),
                StringComparer.Ordinal
            )
            {
                [MessageHeaders.RetryCount] = retries + 1,
            },
        };
        await channel
            .BasicPublishAsync(
                string.Empty,
                registration.Queue,
                mandatory: false,
                properties,
                delivery.Body,
                cancellationToken
            )
            .ConfigureAwait(false);
        await channel
            .BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<bool> IsInInboxAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            return await connection
                .ExecuteScalarAsync<bool>(
                    new CommandDefinition(
                        InboxExistsSql,
                        new { MessageId = messageId, Consumer = registration.Queue },
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);
        }
    }

    private async Task AddToInboxAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            await connection
                .ExecuteAsync(
                    new CommandDefinition(
                        InboxInsertSql,
                        new
                        {
                            MessageId = messageId,
                            Consumer = registration.Queue,
                            registration.MessageType,
                            ProcessedAt = timeProvider.GetUtcNow(),
                        },
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);
        }
    }

    private static string? HeaderString(IReadOnlyBasicProperties properties, string name) =>
        properties.Headers?.TryGetValue(name, out var value) == true
            ? value switch
            {
                byte[] bytes => Encoding.UTF8.GetString(bytes),
                string text => text,
                _ => null,
            }
            : null;

    private static int HeaderInt(IReadOnlyBasicProperties properties, string name) =>
        properties.Headers?.TryGetValue(name, out var value) == true
            ? value switch
            {
                int number => number,
                long number => (int)number,
                _ => 0,
            }
            : 0;

    [LoggerMessage(Level = LogLevel.Information, Message = "Consuming {MessageType} from {Queue}")]
    private partial void LogConsuming(string queue, string messageType);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {MessageType} {MessageId}")]
    private partial void LogHandled(string messageType, Guid messageId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Skipped {MessageType} {MessageId}: already handled (inbox)"
    )]
    private partial void LogDuplicate(string messageType, Guid messageId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Handling {MessageType} {MessageId} failed; retry {Retry} in {DelaySeconds} s"
    )]
    private partial void LogRetrying(
        Exception exception,
        string messageType,
        Guid messageId,
        int retry,
        double delaySeconds
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Handling {MessageType} {MessageId} failed after {Retries} retries; moved to the dead-letter queue"
    )]
    private partial void LogDeadLettered(
        Exception exception,
        string messageType,
        Guid messageId,
        int retries
    );

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "{MessageType} {MessageId} cannot be read; moved to the dead-letter queue"
    )]
    private partial void LogPoison(Exception exception, string messageType, Guid messageId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Message without a valid id on {Queue} ({MessageId}); moved to the dead-letter queue"
    )]
    private partial void LogUnreadable(string queue, string? messageId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cannot consume from {Queue}; reconnecting")]
    private partial void LogConnectFailed(Exception exception, string queue);
}
