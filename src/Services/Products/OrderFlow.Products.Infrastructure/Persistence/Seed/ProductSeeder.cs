using Microsoft.EntityFrameworkCore;
using OrderFlow.BuildingBlocks.Infrastructure.Persistence;
using OrderFlow.Products.Domain.Products;

namespace OrderFlow.Products.Infrastructure.Persistence.Seed;

internal static class ProductSeeder
{
    private const string ResourceName =
        "OrderFlow.Products.Infrastructure.Persistence.Seed.products.json";

    public static async Task SeedAsync(
        DbContext context,
        TimeProvider timeProvider,
        CancellationToken cancellationToken
    )
    {
        var products = context.Set<Product>();
        if (await products.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var seed = await SeedData
            .ReadAsync<ProductSeed>(typeof(ProductSeeder).Assembly, ResourceName, cancellationToken)
            .ConfigureAwait(false);
        var now = timeProvider.GetUtcNow();

        products.AddRange(
            seed.Select(item =>
                Product.Create(
                    item.Id,
                    item.Name,
                    item.Description,
                    item.Category,
                    item.Price,
                    item.Currency,
                    item.Images,
                    item.Specifications.Select(spec => new ProductSpecification(
                        spec.Name,
                        spec.Value
                    )),
                    now
                )
            )
        );
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private sealed record ProductSeed(
        Guid Id,
        string Name,
        string Description,
        string Category,
        decimal Price,
        string Currency,
        IReadOnlyList<string> Images,
        IReadOnlyList<SpecificationSeed> Specifications
    );

    private sealed record SpecificationSeed(string Name, string Value);
}
