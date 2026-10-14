using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Payments.Application.Wallets.GetWalletEventStream;
using OrderFlow.Payments.Domain.Wallets;
using OrderFlow.Payments.Infrastructure.EventStore;

namespace OrderFlow.Payments.Infrastructure.Wallets.GetWalletEventStream;

internal sealed class GetWalletEventStreamQueryHandler(
    NpgsqlDataSource dataSource,
    IOptions<EventStoreOptions> options
) : IQueryHandler<GetWalletEventStreamQuery, WalletEventStreamDto>
{
    private const string Sql = """
        SELECT coalesce(max(version), 0) FROM events WHERE stream_id = @StreamId;

        SELECT version AS Version, schema_version AS SchemaVersion, created_at AS CreatedAt,
               state::text AS State
        FROM snapshots
        WHERE stream_id = @StreamId;

        SELECT global_position AS Position, version AS Version, event_id AS EventId,
               event_type AS Type, occurred_at AS OccurredAt, data::text AS Data
        FROM events
        WHERE stream_id = @StreamId
        ORDER BY version DESC
        LIMIT @Limit;
        """;

    public async Task<Result<WalletEventStreamDto>> HandleAsync(
        GetWalletEventStreamQuery query,
        CancellationToken cancellationToken
    )
    {
        var streamId = Wallet.StreamIdFor(query.OwnerId);
        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var reader = await connection
                .QueryMultipleAsync(
                    new CommandDefinition(
                        Sql,
                        new { StreamId = streamId, Limit = GetWalletEventStreamQuery.MaxEvents },
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);
            await using (reader.ConfigureAwait(false))
            {
                var version = await reader.ReadSingleAsync<int>().ConfigureAwait(false);
                var snapshot = await reader
                    .ReadSingleOrDefaultAsync<SnapshotRow>()
                    .ConfigureAwait(false);
                var events = await reader.ReadAsync<EventRow>().ConfigureAwait(false);

                return Result.Success(
                    new WalletEventStreamDto(
                        streamId,
                        version,
                        options.Value.SnapshotInterval,
                        snapshot?.ToDto(),
                        version - (snapshot?.Version ?? 0),
                        [.. events.Select(row => row.ToDto())]
                    )
                );
            }
        }
    }

    internal sealed class SnapshotRow
    {
        public int Version { get; init; }

        public int SchemaVersion { get; init; }

        public DateTimeOffset CreatedAt { get; init; }

        public string State { get; init; } = string.Empty;

        public SnapshotInfoDto ToDto() =>
            new(Version, SchemaVersion, CreatedAt, JsonSerializer.Deserialize<JsonElement>(State));
    }

    internal sealed class EventRow
    {
        public long Position { get; init; }

        public int Version { get; init; }

        public Guid EventId { get; init; }

        public string Type { get; init; } = string.Empty;

        public DateTimeOffset OccurredAt { get; init; }

        public string Data { get; init; } = string.Empty;

        public StoredEventDto ToDto() =>
            new(
                Position,
                Version,
                EventId,
                Type,
                OccurredAt,
                JsonSerializer.Deserialize<JsonElement>(Data)
            );
    }
}
