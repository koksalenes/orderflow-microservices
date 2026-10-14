using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Infrastructure.Persistence;

namespace OrderFlow.Products.Infrastructure;

public static class ProductsInfrastructureExtensions
{
    public static IServiceCollection AddProductsInfrastructure(this IServiceCollection services)
    {
        services.AddPostgres();

        return services;
    }
}
