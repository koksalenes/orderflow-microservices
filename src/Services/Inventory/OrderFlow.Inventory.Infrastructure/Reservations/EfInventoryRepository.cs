using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Application.Reservations;
using OrderFlow.Inventory.Domain.Reservations;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Infrastructure.Persistence;

namespace OrderFlow.Inventory.Infrastructure.Reservations;

internal sealed class EfInventoryRepository(InventoryDbContext context) : IInventoryRepository
{
    public async Task<IReadOnlyList<StockItem>> GetStockItemsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken
    ) =>
        await context
            .StockItems.Where(item => productIds.Contains(item.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task<StockReservation?> GetReservationAsync(
        Guid orderId,
        CancellationToken cancellationToken
    ) =>
        context.StockReservations.SingleOrDefaultAsync(
            reservation => reservation.Id == orderId,
            cancellationToken
        );

    public void AddReservation(StockReservation reservation) =>
        context.StockReservations.Add(reservation);
}
