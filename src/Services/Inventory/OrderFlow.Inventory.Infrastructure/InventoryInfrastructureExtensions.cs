using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Infrastructure.Persistence;
using OrderFlow.Inventory.Application;
using OrderFlow.Inventory.Application.Stock;
using OrderFlow.Inventory.Application.Stock.GetStockLevel;
using OrderFlow.Inventory.Application.Stock.GetStockLevels;
using OrderFlow.Inventory.Infrastructure.Persistence;
using OrderFlow.Inventory.Infrastructure.Persistence.Seed;
using OrderFlow.Inventory.Infrastructure.Stock.GetStockLevel;
using OrderFlow.Inventory.Infrastructure.Stock.GetStockLevels;

namespace OrderFlow.Inventory.Infrastructure;

public static class InventoryInfrastructureExtensions
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services)
    {
        services.AddInventoryApplication();
        services.AddPostgresDbContext<InventoryDbContext>(
            (_, options) =>
                options.UseAsyncSeeding(
                    (context, _, cancellationToken) =>
                        StockSeeder.SeedAsync(context, cancellationToken)
                )
        );

        services.AddQueryHandler<
            GetStockLevelsQuery,
            IReadOnlyList<StockLevelDto>,
            GetStockLevelsQueryHandler
        >();
        services.AddQueryHandler<GetStockLevelQuery, StockLevelDto, GetStockLevelQueryHandler>();

        return services;
    }

    public static Task MigrateInventoryDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default
    ) => services.MigrateDatabaseAsync<InventoryDbContext>(cancellationToken);
}
