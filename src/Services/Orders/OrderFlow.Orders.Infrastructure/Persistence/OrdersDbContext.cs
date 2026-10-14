using Microsoft.EntityFrameworkCore;
using OrderFlow.BuildingBlocks.Infrastructure.Messaging;
using OrderFlow.Orders.Domain.Orders;

namespace OrderFlow.Orders.Infrastructure.Persistence;

internal sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options)
    : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly)
            .AddOutboxMessages()
            .AddInboxMessages();
}
