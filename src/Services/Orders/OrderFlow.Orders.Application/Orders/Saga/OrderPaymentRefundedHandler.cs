using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Orders.Contracts.Orders;
using OrderFlow.Orders.Domain.Orders;
using OrderFlow.Payments.Contracts.Orders;

namespace OrderFlow.Orders.Application.Orders.Saga;

internal sealed class OrderPaymentRefundedHandler(
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    IOutbox outbox,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<OrderPaymentRefunded>
{
    public async Task HandleAsync(
        OrderPaymentRefunded message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        var order = await orders.GetAsync(message.OrderId, cancellationToken).ConfigureAwait(false);
        var now = timeProvider.GetUtcNow();
        if (order is null || !order.MarkRefunded(now))
        {
            return;
        }

        outbox.EnqueueStatus(order, OrderStatuses.Refunded, now, OrderScenarios.LostParcel);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
