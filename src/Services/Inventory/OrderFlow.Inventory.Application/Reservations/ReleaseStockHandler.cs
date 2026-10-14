using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Inventory.Contracts.Reservations;

namespace OrderFlow.Inventory.Application.Reservations;

internal sealed class ReleaseStockHandler(
    IInventoryRepository inventory,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<ReleaseStock>
{
    public async Task HandleAsync(
        ReleaseStock message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        var reservation = await inventory
            .GetReservationAsync(message.OrderId, cancellationToken)
            .ConfigureAwait(false);
        if (
            reservation is null
            || !reservation.Release(message.ReasonCode, timeProvider.GetUtcNow())
        )
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
            items.GetValueOrDefault(line.ProductId)?.Release(line.Quantity);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
