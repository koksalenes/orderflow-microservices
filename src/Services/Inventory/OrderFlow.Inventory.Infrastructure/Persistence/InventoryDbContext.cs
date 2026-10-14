using Microsoft.EntityFrameworkCore;
using OrderFlow.BuildingBlocks.Infrastructure.Messaging;
using OrderFlow.Inventory.Domain.Reservations;
using OrderFlow.Inventory.Domain.Stock;

namespace OrderFlow.Inventory.Infrastructure.Persistence;

internal sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options)
    : DbContext(options)
{
    public DbSet<StockItem> StockItems => Set<StockItem>();

    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly)
            .AddOutboxMessages()
            .AddInboxMessages();
}
