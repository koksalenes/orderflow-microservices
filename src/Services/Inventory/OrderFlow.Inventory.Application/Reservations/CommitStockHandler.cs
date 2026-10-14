using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Inventory.Contracts.Reservations;

namespace OrderFlow.Inventory.Application.Reservations;

internal sealed class CommitStockHandler(
    IInventoryRepository inventory,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<CommitStock>
{
    public async Task HandleAsync(
        CommitStock message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        var reservation = await inventory
            .GetReservationAsync(message.OrderId, cancellationToken)
            .ConfigureAwait(false);
        if (reservation is null || !reservation.Commit(timeProvider.GetUtcNow()))
        {
            return;
        }

        var items = (
            await inventory
                .GetStockItemsAsync(
                    [.. reservation.Lines.Select(line => line.ProductId)],
                    cancellationToken
                )
                .ConfigureAwait(false)
        ).ToDictionary(item => item.Id);
        foreach (var line in reservation.Lines)
        {
            items.GetValueOrDefault(line.ProductId)?.Commit(line.Quantity);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
