using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Inventory.Contracts.Reservations;
using OrderFlow.Payments.Contracts.Orders;

namespace OrderFlow.Orders.Application.Orders.Saga;

internal sealed class StockReservedHandler(
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    IOutbox outbox,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<StockReserved>
{
    public async Task HandleAsync(
        StockReserved message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        var order = await orders.GetAsync(message.OrderId, cancellationToken).ConfigureAwait(false);
        if (order is null || !order.MarkStockReserved(timeProvider.GetUtcNow()))
        {
            return;
        }

        outbox.Enqueue(
            ChargeOrder.MessageType,
            new ChargeOrder(
                order.Id,
                order.CustomerId,
                order.Total,
                order.Currency,
                order.MethodCode(),
                order.CardPaymentId
            )
        );
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
