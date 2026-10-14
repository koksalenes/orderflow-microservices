using Microsoft.EntityFrameworkCore;
using OrderFlow.Products.Domain.Products;

namespace OrderFlow.Products.Infrastructure.Persistence;

internal sealed class ProductsDbContext(DbContextOptions<ProductsDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductsDbContext).Assembly);
}
