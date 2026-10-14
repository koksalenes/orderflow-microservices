using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Payments.Contracts.Orders;

namespace OrderFlow.Orders.Application.Orders.Saga;

internal sealed class OrderPaymentCapturedHandler(
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    FulfillmentSchedule schedule,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<OrderPaymentCaptured>
{
    public async Task HandleAsync(
        OrderPaymentCaptured message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        var order = await orders.GetAsync(message.OrderId, cancellationToken).ConfigureAwait(false);
        var now = timeProvider.GetUtcNow();
        if (order is null || !order.MarkPaid(message.Reference, now, now + schedule.ConfirmAfter))
        {
            return;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
