using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Infrastructure.Persistence;

namespace OrderFlow.Orders.Infrastructure;

public static class OrdersInfrastructureExtensions
{
    public static IServiceCollection AddOrdersInfrastructure(this IServiceCollection services)
    {
        services.AddPostgres();

        return services;
    }
}
