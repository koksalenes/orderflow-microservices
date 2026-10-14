using System.Diagnostics;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.BuildingBlocks.Infrastructure.Messaging;
using OrderFlow.Payments.Application.Wallets;
using OrderFlow.Payments.Domain.EventSourcing;
using OrderFlow.Payments.Domain.Wallets;
using OrderFlow.Payments.Infrastructure.EventStore;

namespace OrderFlow.Payments.Infrastructure.Wallets;

internal sealed class EventSourcedWalletRepository(
    NpgsqlDataSource dataSource,
    IOptions<EventStoreOptions> options,
    Outbox outbox,
    TimeProvider timeProvider
) : IWalletRepository
{
    private const string SnapshotSql = """
        SELECT version AS Version, schema_version AS SchemaVersion, state::text AS State
        FROM snapshots
        WHERE stream_id = @StreamId;
        """;

    private const string EventsSql = """
        SELECT event_type AS EventType, data::text AS Data
        FROM events
        WHERE stream_id = @StreamId AND version > @AfterVersion
        ORDER BY version;
        """;

    private const string AppendSql = """
        INSERT INTO events (stream_id, version, event_id, event_type, data, metadata, occurred_at)
        VALUES (@StreamId, @Version, @EventId, @EventType, @Data::jsonb, @Metadata::jsonb, @OccurredAt);
        """;

    private const string SaveSnapshotSql = """
        INSERT INTO snapshots (stream_id, version, schema_version, state, created_at)
        VALUES (@StreamId, @Version, @SchemaVersion, @State::jsonb, @CreatedAt)
        ON CONFLICT (stream_id) DO UPDATE
        SET version = excluded.version, schema_version = excluded.schema_version,
            state = excluded.state, created_at = excluded.created_at;
        """;

    public async Task<Wallet?> GetAsync(string ownerId, CancellationToken cancellationToken)
    {
        var streamId = Wallet.StreamIdFor(ownerId);
        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var snapshotRow = await connection
                .QuerySingleOrDefaultAsync<SnapshotRow>(
                    new CommandDefinition(
                        SnapshotSql,
                        new { StreamId = streamId },
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);

            var snapshot =
                snapshotRow?.SchemaVersion == WalletSnapshot.SchemaVersion
                    ? EventTypeRegistry.DeserializeState<WalletSnapshot>(snapshotRow.State)
                    : null;
            var snapshotVersion = snapshot is null ? 0 : snapshotRow!.Version;

            var events = await connection
                .QueryAsync<EventRow>(
                    new CommandDefinition(
                        EventsSql,
                        new { StreamId = streamId, AfterVersion = snapshotVersion },
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);
            var history = events
                .Select(row => EventTypeRegistry.Deserialize(row.EventType, row.Data))
                .ToList();

            return snapshot is null && history.Count == 0
                ? null
                : Wallet.Rehydrate(snapshot, snapshotVersion, history);
        }
    }

    public async Task<Result> SaveAsync(Wallet wallet, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(wallet);

        if (wallet.PendingEvents.Count == 0)
        {
            return Result.Success();
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
                try
                {
                    await AppendAndProjectAsync(connection, transaction, wallet, cancellationToken)
                        .ConfigureAwait(false);
                    await SnapshotIfDueAsync(connection, transaction, wallet, cancellationToken)
                        .ConfigureAwait(false);
                    await outbox
                        .WriteAsync(connection, transaction, cancellationToken)
                        .ConfigureAwait(false);
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (PostgresException exception)
                    when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    outbox.Clear();
                    return Result.Failure(ToError(exception, wallet));
                }
            }
        }

        wallet.MarkPendingEventsAsStored();
        return Result.Success();
    }

    private async Task AppendAndProjectAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Wallet wallet,
        CancellationToken cancellationToken
    )
    {
        var version = wallet.Version;
        var metadata = EventTypeRegistry.SerializeState(
            new { traceId = Activity.Current?.TraceId.ToHexString() }
        );

        foreach (var domainEvent in wallet.PendingEvents)
        {
            version++;
            await connection
                .ExecuteAsync(
                    new CommandDefinition(
                        AppendSql,
                        new
                        {
                            wallet.StreamId,
                            Version = version,
                            EventId = EventIdOf(domainEvent),
                            EventType = EventTypeRegistry.NameOf(domainEvent),
                            Data = EventTypeRegistry.Serialize(domainEvent),
                            Metadata = metadata,
                            domainEvent.OccurredAt,
                        },
                        transaction,
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);

            await WalletProjector
                .ProjectAsync(
                    connection,
                    transaction,
                    wallet.OwnerId,
                    domainEvent,
                    version,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }
    }

    private async Task SnapshotIfDueAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Wallet wallet,
        CancellationToken cancellationToken
    )
    {
        var interval = options.Value.SnapshotInterval;
        var newVersion = wallet.Version + wallet.PendingEvents.Count;
        if (newVersion / interval == wallet.Version / interval)
        {
            return;
        }

        await connection
            .ExecuteAsync(
                new CommandDefinition(
                    SaveSnapshotSql,
                    new
                    {
                        wallet.StreamId,
                        Version = newVersion,
                        WalletSnapshot.SchemaVersion,
                        State = EventTypeRegistry.SerializeState(wallet.ToSnapshot()),
                        CreatedAt = timeProvider.GetUtcNow(),
                    },
                    transaction,
                    cancellationToken: cancellationToken
                )
            )
            .ConfigureAwait(false);
    }

    private Guid EventIdOf(IDomainEvent domainEvent) =>
        domainEvent switch
        {
            FundsDeposited deposited => deposited.DepositId,
            DepositReversed reversed => reversed.ReversalId,
            FundsWithdrawn withdrawn => withdrawn.OrderId,
            FundsRefunded refunded => refunded.RefundId,
            _ => Guid.CreateVersion7(timeProvider.GetUtcNow()),
        };

    private static Error ToError(PostgresException exception, Wallet wallet) =>
        exception.ConstraintName == EventStoreConstraints.EventId
        && wallet.PendingEvents.OfType<FundsDeposited>().FirstOrDefault() is { } deposited
            ? WalletErrors.DuplicateDeposit(deposited.DepositId)
            : WalletErrors.ConcurrencyConflict(wallet.StreamId);

    internal sealed class SnapshotRow
    {
        public int Version { get; init; }

        public int SchemaVersion { get; init; }

        public string State { get; init; } = string.Empty;
    }

    internal sealed class EventRow
    {
        public string EventType { get; init; } = string.Empty;

        public string Data { get; init; } = string.Empty;
    }
}
