using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrderFlow.BuildingBlocks.ServiceDefaults.Correlation;

internal static class CorrelationExtensions
{
    public static IServiceCollection AddCorrelation(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddTransient<CorrelationIdHandler>();
        services.ConfigureHttpClientDefaults(client =>
            client.AddHttpMessageHandler<CorrelationIdHandler>()
        );

        return services;
    }
}
