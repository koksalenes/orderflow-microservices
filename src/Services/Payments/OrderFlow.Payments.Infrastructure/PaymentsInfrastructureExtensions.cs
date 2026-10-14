using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Infrastructure.Persistence;

namespace OrderFlow.Payments.Infrastructure;

public static class PaymentsInfrastructureExtensions
{
    public static IServiceCollection AddPaymentsInfrastructure(this IServiceCollection services)
    {
        services.AddPostgres();

        return services;
    }
}
