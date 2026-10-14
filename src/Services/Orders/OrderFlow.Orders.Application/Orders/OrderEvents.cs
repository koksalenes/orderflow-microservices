using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Orders.Contracts.Orders;
using OrderFlow.Orders.Domain.Orders;
using OrderFlow.Payments.Contracts.Orders;

namespace OrderFlow.Orders.Application.Orders;

internal static class OrderEvents
{
    public static void EnqueueStatus(
        this IOutbox outbox,
        Order order,
        string status,
        DateTimeOffset now,
        string? reasonCode = null
    ) =>
        outbox.Enqueue(
            OrderStatusChanged.MessageType,
            new OrderStatusChanged(Snapshot(order), status, reasonCode, now)
        );

    public static string MethodCode(this Order order) =>
        order.PaymentMethod == PaymentMethod.Card ? PaymentMethods.Card : PaymentMethods.Wallet;

    private static OrderSnapshot Snapshot(Order order) =>
        new(
            order.Id,
            order.Number,
            order.CustomerId,
            order.CustomerEmail,
            order.CustomerName,
            order.Locale,
            [
                .. order.Lines.Select(line => new OrderLineSnapshot(
                    line.ProductId,
                    line.Name,
                    line.UnitPrice,
                    line.Quantity,
                    line.LineTotal,
                    line.ImageUrl
                )),
            ],
            order.Subtotal,
            order.ShippingFee,
            order.Total,
            order.Currency,
            order.MethodCode(),
            order.CardBrand,
            order.CardLast4,
            new OrderAddressSnapshot(
                order.ShippingAddress.FullName,
                order.ShippingAddress.Line1,
                order.ShippingAddress.Line2,
                order.ShippingAddress.City,
                order.ShippingAddress.PostalCode,
                order.ShippingAddress.Country,
                order.ShippingAddress.Phone
            ),
            order.TrackingNumber,
            order.InvoiceNumber,
            order.PlacedAt
        );
}
