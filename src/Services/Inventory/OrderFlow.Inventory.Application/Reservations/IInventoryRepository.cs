using OrderFlow.Inventory.Domain.Reservations;
using OrderFlow.Inventory.Domain.Stock;

namespace OrderFlow.Inventory.Application.Reservations;

public interface IInventoryRepository
{
    Task<IReadOnlyList<StockItem>> GetStockItemsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken
    );

    Task<StockReservation?> GetReservationAsync(Guid orderId, CancellationToken cancellationToken);

    void AddReservation(StockReservation reservation);
}
