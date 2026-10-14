using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Payments.Infrastructure.EventStore;

public sealed class EventStoreOptions
{
    public const string SectionName = "EventStore";

    [Range(1, 10_000)]
    public int SnapshotInterval { get; init; } = 100;
}
