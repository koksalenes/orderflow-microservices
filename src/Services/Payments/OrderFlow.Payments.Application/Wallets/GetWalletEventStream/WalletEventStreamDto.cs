using System.Text.Json;

namespace OrderFlow.Payments.Application.Wallets.GetWalletEventStream;

public sealed record WalletEventStreamDto(
    string StreamId,
    int Version,
    int SnapshotInterval,
    SnapshotInfoDto? Snapshot,
    int EventsSinceSnapshot,
    IReadOnlyList<StoredEventDto> Events
);

public sealed record SnapshotInfoDto(
    int Version,
    int SchemaVersion,
    DateTimeOffset CreatedAt,
    JsonElement State
);

public sealed record StoredEventDto(
    long Position,
    int Version,
    Guid EventId,
    string Type,
    DateTimeOffset OccurredAt,
    JsonElement Data
);
