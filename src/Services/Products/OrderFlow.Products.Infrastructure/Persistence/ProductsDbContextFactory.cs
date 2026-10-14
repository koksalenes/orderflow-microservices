using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Products.Infrastructure.Persistence;

internal sealed class ProductsDbContextFactory : IDesignTimeDbContextFactory<ProductsDbContext>
{
    public ProductsDbContext CreateDbContext(string[] args) =>
        new(
            new DbContextOptionsBuilder<ProductsDbContext>()
                .UseNpgsql("Host=localhost;Database=products_db")
                .UseSnakeCaseNamingConvention()
                .Options
        );
}
