using Microsoft.EntityFrameworkCore;
using OrderFlow.BuildingBlocks.Infrastructure.Persistence;
using OrderFlow.Inventory.Domain.Stock;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Seed;

internal static class StockSeeder
{
    private const string ResourceName =
        "OrderFlow.Inventory.Infrastructure.Persistence.Seed.stock.json";

    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        var stockItems = context.Set<StockItem>();
        if (await stockItems.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var seed = await SeedData
            .ReadAsync<StockSeed>(typeof(StockSeeder).Assembly, ResourceName, cancellationToken)
            .ConfigureAwait(false);

        stockItems.AddRange(seed.Select(item => StockItem.Create(item.ProductId, item.Quantity)));
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private sealed record StockSeed(Guid ProductId, int Quantity);
}
