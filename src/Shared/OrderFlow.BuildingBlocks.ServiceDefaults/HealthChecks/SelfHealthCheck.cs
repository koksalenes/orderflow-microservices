using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrderFlow.BuildingBlocks.ServiceDefaults.HealthChecks;

internal sealed class SelfHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(HealthCheckResult.Healthy());
}
