using System.Globalization;
using System.Security.Cryptography;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Inventory.Contracts.Reservations;
using OrderFlow.Orders.Contracts.Orders;
using OrderFlow.Orders.Domain.Orders;
using OrderFlow.Payments.Contracts.Orders;

namespace OrderFlow.Orders.Application.Orders.Saga;

public sealed class OrderFulfillment(
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    IOutbox outbox,
    FulfillmentSchedule schedule,
    TimeProvider timeProvider
)
{
    public async Task AdvanceAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await orders.GetAsync(orderId, cancellationToken).ConfigureAwait(false);
        var now = timeProvider.GetUtcNow();
        if (order?.NextActionAt is not { } due || due > now)
        {
            return;
        }

        switch (order.Status)
        {
            case OrderStatus.Paid:
                order.Confirm(now, now + schedule.ShipAfter);
                outbox.EnqueueStatus(order, OrderStatuses.Confirmed, now);
                break;

            case OrderStatus.Confirmed:
                order.Ship(TrackingNumber(), now, now + schedule.DeliverAfter);
                outbox.Enqueue(CommitStock.MessageType, new CommitStock(order.Id));
                outbox.EnqueueStatus(order, OrderStatuses.Shipped, now);
                break;

            case OrderStatus.Shipped when order.Scenario == OrderScenarios.LostParcel:
                order.FailDelivery(Guid.CreateVersion7(now), now);
                outbox.Enqueue(
                    RefundOrderPayment.MessageType,
                    new RefundOrderPayment(
                        order.Id,
                        order.RefundId!.Value,
                        order.CustomerId,
                        order.MethodCode(),
                        order.CardPaymentId
                    )
                );
                break;

            case OrderStatus.Shipped:
                order.Deliver(now, now + schedule.InvoiceAfter);
                outbox.EnqueueStatus(order, OrderStatuses.Delivered, now);
                break;

            case OrderStatus.Delivered:
                order.Invoice(InvoiceNumber(order, now), now);
                outbox.EnqueueStatus(order, OrderStatuses.Invoiced, now);
                break;

            case OrderStatus.Placed:
            case OrderStatus.AwaitingPayment:
            case OrderStatus.Completed:
            case OrderStatus.Cancelled:
            case OrderStatus.Refunding:
            case OrderStatus.Refunded:
            default:
                return;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string TrackingNumber() =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"OFX{RandomNumberGenerator.GetInt32(100_000_000, 999_999_999)}TR"
        );

    private static string InvoiceNumber(Order order, DateTimeOffset now) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"INV-{now:yyyy}-{order.Id.ToString("N")[^8..].ToUpperInvariant()}"
        );
}
