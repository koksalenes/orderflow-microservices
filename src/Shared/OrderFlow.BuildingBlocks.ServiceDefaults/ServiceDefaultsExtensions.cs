using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderFlow.BuildingBlocks.ServiceDefaults.HealthChecks;
using OrderFlow.BuildingBlocks.ServiceDefaults.Logging;
using OrderFlow.BuildingBlocks.ServiceDefaults.Shutdown;
using OrderFlow.BuildingBlocks.ServiceDefaults.Telemetry;

namespace OrderFlow.BuildingBlocks.ServiceDefaults;

public static class ServiceDefaultsExtensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddSerilogLogging();
        builder.AddTelemetry();
        builder.Services.AddGracefulShutdown();

        builder
            .Services.AddHealthChecks()
            .AddCheck<SelfHealthCheck>("self", tags: [HealthCheckTags.Live])
            .AddCheck<ShutdownHealthCheck>("shutdown");

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks(
                "/health/live",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains(HealthCheckTags.Live),
                }
            )
            .AllowAnonymous();

        app.MapHealthChecks(
                "/health/ready",
                new HealthCheckOptions
                {
                    Predicate = check => !check.Tags.Contains(HealthCheckTags.Live),
                }
            )
            .AllowAnonymous();

        return app;
    }
}
