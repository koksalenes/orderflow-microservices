using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Inventory.Infrastructure.Persistence;

internal sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args) =>
        new(
            new DbContextOptionsBuilder<InventoryDbContext>()
                .UseNpgsql("Host=localhost;Database=inventory_db")
                .UseSnakeCaseNamingConvention()
                .Options
        );
}
