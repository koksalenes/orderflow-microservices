namespace OrderFlow.Payments.Infrastructure.Persistence;

internal sealed class StoredEvent
{
    public long GlobalPosition { get; init; }

    public string StreamId { get; init; } = string.Empty;

    public int Version { get; init; }

    public Guid EventId { get; init; }

    public string EventType { get; init; } = string.Empty;

    public string Data { get; init; } = string.Empty;

    public string? Metadata { get; init; }

    public DateTimeOffset OccurredAt { get; init; }

    public DateTimeOffset RecordedAt { get; init; }
}

internal sealed class StoredSnapshot
{
    public string StreamId { get; init; } = string.Empty;

    public int Version { get; init; }

    public int SchemaVersion { get; init; }

    public string State { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }
}

internal sealed class WalletBalanceRow
{
    public string OwnerId { get; init; } = string.Empty;

    public decimal Balance { get; init; }

    public string Currency { get; init; } = string.Empty;

    public int Version { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}

internal sealed class WalletTransactionRow
{
    public Guid Id { get; init; }

    public string OwnerId { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public decimal BalanceAfter { get; init; }

    public string Currency { get; init; } = string.Empty;

    public string? CardBrand { get; init; }

    public string? CardLast4 { get; init; }

    public string? Status { get; init; }

    public Guid? ReferenceId { get; init; }

    public string? Reason { get; init; }

    public DateTimeOffset OccurredAt { get; init; }
}
