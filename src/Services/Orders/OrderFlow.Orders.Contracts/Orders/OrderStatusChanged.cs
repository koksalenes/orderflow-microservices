namespace OrderFlow.Orders.Contracts.Orders;

public static class OrderStatuses
{
    public const string Placed = "placed";
    public const string Confirmed = "confirmed";
    public const string Shipped = "shipped";
    public const string Delivered = "delivered";
    public const string Invoiced = "invoiced";
    public const string Cancelled = "cancelled";
    public const string Refunded = "refunded";
}

public sealed record OrderStatusChanged(
    OrderSnapshot Order,
    string Status,
    string? ReasonCode,
    DateTimeOffset OccurredAt
)
{
    public const string MessageType = "orders.order_status_changed.v1";
}

public sealed record OrderSnapshot(
    Guid OrderId,
    string OrderNumber,
    string CustomerId,
    string? CustomerEmail,
    string? CustomerName,
    string Locale,
    IReadOnlyList<OrderLineSnapshot> Lines,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Total,
    string Currency,
    string PaymentMethod,
    string? CardBrand,
    string? CardLast4,
    OrderAddressSnapshot ShippingAddress,
    string? TrackingNumber,
    string? InvoiceNumber,
    DateTimeOffset PlacedAt
);

public sealed record OrderLineSnapshot(
    Guid ProductId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    Uri? ImageUrl
);

public sealed record OrderAddressSnapshot(
    string FullName,
    string Line1,
    string? Line2,
    string City,
    string PostalCode,
    string Country,
    string Phone
);
