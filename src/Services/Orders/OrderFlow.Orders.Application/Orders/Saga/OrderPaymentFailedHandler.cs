using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Inventory.Contracts.Reservations;
using OrderFlow.Orders.Contracts.Orders;
using OrderFlow.Payments.Contracts.Orders;

namespace OrderFlow.Orders.Application.Orders.Saga;

internal sealed class OrderPaymentFailedHandler(
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    IOutbox outbox,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<OrderPaymentFailed>
{
    public async Task HandleAsync(
        OrderPaymentFailed message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        var order = await orders.GetAsync(message.OrderId, cancellationToken).ConfigureAwait(false);
        var now = timeProvider.GetUtcNow();
        if (order is null || !order.FailPayment(message.ReasonCode, now))
        {
            return;
        }

        outbox.Enqueue(ReleaseStock.MessageType, new ReleaseStock(order.Id, message.ReasonCode));
        outbox.EnqueueStatus(order, OrderStatuses.Cancelled, now, message.ReasonCode);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
