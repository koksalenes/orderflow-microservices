using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Infrastructure.Persistence;

namespace OrderFlow.Inventory.Infrastructure;

public static class InventoryInfrastructureExtensions
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services)
    {
        services.AddPostgres();

        return services;
    }
}
