using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace OrderFlow.BuildingBlocks.ServiceDefaults.HealthChecks;

internal sealed class ShutdownHealthCheck(IHostApplicationLifetime lifetime) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    ) =>
        Task.FromResult(
            lifetime.ApplicationStopping.IsCancellationRequested
                ? HealthCheckResult.Unhealthy("The host is shutting down.")
                : HealthCheckResult.Healthy()
        );
}
