using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace OrderFlow.BuildingBlocks.ServiceDefaults.Shutdown;

internal static class GracefulShutdownExtensions
{
    public static IServiceCollection AddGracefulShutdown(this IServiceCollection services)
    {
        services
            .AddOptions<GracefulShutdownOptions>()
            .BindConfiguration(GracefulShutdownOptions.SectionName)
            .ValidateDataAnnotations()
            .Validate(
                options => options.DrainDelaySeconds < options.TimeoutSeconds,
                "GracefulShutdown:DrainDelaySeconds must be lower than GracefulShutdown:TimeoutSeconds."
            )
            .ValidateOnStart();

        services
            .AddOptions<HostOptions>()
            .Configure<IOptions<GracefulShutdownOptions>>(
                (host, shutdown) =>
                    host.ShutdownTimeout = TimeSpan.FromSeconds(shutdown.Value.TimeoutSeconds)
            );

        services.AddHostedService<ShutdownDrainService>();

        return services;
    }
}
