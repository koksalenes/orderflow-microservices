using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;
using Dapper;
using Npgsql;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.ServiceDefaults.Correlation;

namespace OrderFlow.BuildingBlocks.Infrastructure.Messaging;

public sealed class Outbox(TimeProvider timeProvider) : IOutbox
{
    private const string InsertSql = """
        INSERT INTO outbox_messages (id, type, payload, occurred_at, correlation_id, trace_parent, attempts)
        VALUES (@Id, @Type, @Payload::jsonb, @OccurredAt, @CorrelationId, @TraceParent, 0);
        """;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly List<OutboxMessage> _pending = [];

    public void Enqueue<TMessage>(string messageType, TMessage message)
        where TMessage : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageType);
        ArgumentNullException.ThrowIfNull(message);

        var now = timeProvider.GetUtcNow();
        _pending.Add(
            new OutboxMessage
            {
                Id = Guid.CreateVersion7(now),
                Type = messageType,
                Payload = JsonSerializer.Serialize(message, JsonOptions),
                OccurredAt = now,
                CorrelationId = CorrelationContext.Current,
                TraceParent = Activity.Current?.Id,
            }
        );
    }

    public async Task WriteAsync(
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(connection);

        foreach (var message in _pending)
        {
            await connection
                .ExecuteAsync(
                    new CommandDefinition(
                        InsertSql,
                        message,
                        transaction,
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);
        }

        _pending.Clear();
    }

    public void Clear() => _pending.Clear();

    public async Task FlushAsync(NpgsqlDataSource dataSource, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dataSource);

        if (_pending.Count == 0)
        {
            return;
        }

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
                await WriteAsync(connection, transaction, cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
