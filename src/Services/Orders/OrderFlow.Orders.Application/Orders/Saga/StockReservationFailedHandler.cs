using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Inventory.Contracts.Reservations;
using OrderFlow.Orders.Contracts.Orders;
using OrderFlow.Orders.Domain.Orders;
using OrderFlow.Payments.Contracts.Orders;

namespace OrderFlow.Orders.Application.Orders.Saga;

internal sealed class StockReservationFailedHandler(
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    IOutbox outbox,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<StockReservationFailed>
{
    public async Task HandleAsync(
        StockReservationFailed message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        var order = await orders.GetAsync(message.OrderId, cancellationToken).ConfigureAwait(false);
        if (order is null)
        {
            return;
        }

        var names = order.Lines.ToDictionary(line => line.ProductId, line => line.Name);
        var detail = string.Join(
            ", ",
            message.Shortages.Select(shortage =>
                $"{names.GetValueOrDefault(shortage.ProductId, shortage.ProductId.ToString())} ({shortage.Available}/{shortage.Requested})"
            )
        );
        var now = timeProvider.GetUtcNow();
        if (!order.RejectStock(message.ReasonCode, detail, now))
        {
            return;
        }

        if (order.PaymentMethod == PaymentMethod.Card && order.CardPaymentId is { } cardPaymentId)
        {
            outbox.Enqueue(
                VoidCardPayment.MessageType,
                new VoidCardPayment(cardPaymentId, order.Id)
            );
        }

        outbox.EnqueueStatus(order, OrderStatuses.Cancelled, now, message.ReasonCode);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
