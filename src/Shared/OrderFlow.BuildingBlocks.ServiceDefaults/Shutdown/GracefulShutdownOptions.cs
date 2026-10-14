using System.ComponentModel.DataAnnotations;

namespace OrderFlow.BuildingBlocks.ServiceDefaults.Shutdown;

public sealed class GracefulShutdownOptions
{
    public const string SectionName = "GracefulShutdown";

    /** Total time the host may spend stopping before it is forced down. */
    [Range(1, 300)]
    public int TimeoutSeconds { get; init; } = 30;

    /** Time to keep serving after readiness turns unhealthy, so the gateway can take the instance out. */
    [Range(0, 60)]
    public int DrainDelaySeconds { get; init; } = 5;
}
