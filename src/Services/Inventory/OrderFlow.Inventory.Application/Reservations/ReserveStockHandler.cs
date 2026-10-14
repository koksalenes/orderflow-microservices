using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Inventory.Contracts.Reservations;
using OrderFlow.Inventory.Domain.Reservations;

namespace OrderFlow.Inventory.Application.Reservations;

internal sealed class ReserveStockHandler(
    IInventoryRepository inventory,
    IUnitOfWork unitOfWork,
    IOutbox outbox,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<ReserveStock>
{
    public const string OutOfStock = "out_of_stock";

    public async Task HandleAsync(
        ReserveStock message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        if (
            await inventory
                .GetReservationAsync(message.OrderId, cancellationToken)
                .ConfigureAwait(false)
            is not null
        )
        {
            return;
        }

        var now = timeProvider.GetUtcNow();
        var lines = message
            .Lines.GroupBy(line => line.ProductId)
            .Select(group => new ReservationLine(group.Key, group.Sum(line => line.Quantity)))
            .ToList();
        var items = (
            await inventory
                .GetStockItemsAsync([.. lines.Select(line => line.ProductId)], cancellationToken)
                .ConfigureAwait(false)
        ).ToDictionary(item => item.Id);

        var shortages = lines
            .Where(line =>
                !items.TryGetValue(line.ProductId, out var item) || !item.CanReserve(line.Quantity)
            )
            .Select(line => new StockShortage(
                line.ProductId,
                line.Quantity,
                items.TryGetValue(line.ProductId, out var item) ? item.Available : 0
            ))
            .ToList();

        if (shortages.Count > 0)
        {
            inventory.AddReservation(
                StockReservation.Rejected(message.OrderId, lines, OutOfStock, now)
            );
            outbox.Enqueue(
                StockReservationFailed.MessageType,
                new StockReservationFailed(message.OrderId, OutOfStock, shortages)
            );
        }
        else
        {
            foreach (var line in lines)
            {
                items[line.ProductId].Reserve(line.Quantity);
            }

            inventory.AddReservation(StockReservation.Reserved(message.OrderId, lines, now));
            outbox.Enqueue(StockReserved.MessageType, new StockReserved(message.OrderId));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
