using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Orders.Infrastructure.Persistence;

internal sealed class OrdersDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args) =>
        new(
            new DbContextOptionsBuilder<OrdersDbContext>()
                .UseNpgsql("Host=localhost;Database=orders_db")
                .UseSnakeCaseNamingConvention()
                .Options
        );
}
