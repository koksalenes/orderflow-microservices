using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Application.Paging;
using OrderFlow.BuildingBlocks.Infrastructure.Persistence;
using OrderFlow.Products.Application;
using OrderFlow.Products.Application.Products.GetProductById;
using OrderFlow.Products.Application.Products.GetProducts;
using OrderFlow.Products.Infrastructure.Persistence;
using OrderFlow.Products.Infrastructure.Persistence.Seed;
using OrderFlow.Products.Infrastructure.Products.GetProductById;
using OrderFlow.Products.Infrastructure.Products.GetProducts;

namespace OrderFlow.Products.Infrastructure;

public static class ProductsInfrastructureExtensions
{
    public static IServiceCollection AddProductsInfrastructure(this IServiceCollection services)
    {
        services.AddProductsApplication();
        services.AddPostgresDbContext<ProductsDbContext>(
            (provider, options) =>
                options.UseAsyncSeeding(
                    (context, _, cancellationToken) =>
                        ProductSeeder.SeedAsync(
                            context,
                            provider.GetRequiredService<TimeProvider>(),
                            cancellationToken
                        )
                )
        );

        services.AddQueryHandler<
            GetProductsQuery,
            PagedResult<ProductSummaryDto>,
            GetProductsQueryHandler
        >();
        services.AddQueryHandler<GetProductByIdQuery, ProductDto, GetProductByIdQueryHandler>();

        return services;
    }

    public static Task MigrateProductsDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default
    ) => services.MigrateDatabaseAsync<ProductsDbContext>(cancellationToken);
}
