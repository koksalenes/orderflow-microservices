using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Gateway.RateLimiting;

internal sealed class GatewayRateLimitOptions
{
    public const string SectionName = "RateLimiting";

    [Range(1, 10_000)]
    public int PermitsPerSecond { get; init; } = 50;

    [Range(1, 10_000)]
    public int Burst { get; init; } = 100;

    [Range(1, 10_000)]
    public int WritePermitsPerSecond { get; init; } = 10;

    [Range(1, 10_000)]
    public int WriteBurst { get; init; } = 20;

    [Range(1, 10_000)]
    public int ConcurrentRequests { get; init; } = 50;
}
