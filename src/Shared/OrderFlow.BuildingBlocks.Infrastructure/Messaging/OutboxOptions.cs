using System.ComponentModel.DataAnnotations;

namespace OrderFlow.BuildingBlocks.Infrastructure.Messaging;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    [Range(100, 60_000)]
    public int PollIntervalMilliseconds { get; init; } = 1000;

    [Range(1, 1000)]
    public int BatchSize { get; init; } = 50;
}
