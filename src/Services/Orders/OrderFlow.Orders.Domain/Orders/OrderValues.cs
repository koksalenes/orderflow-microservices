namespace OrderFlow.Orders.Domain.Orders;

public sealed record OrderLine(
    Guid ProductId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    Uri? ImageUrl
)
{
    public decimal LineTotal => UnitPrice * Quantity;
}

public sealed record ShippingAddress(
    string FullName,
    string Line1,
    string? Line2,
    string City,
    string PostalCode,
    string Country,
    string Phone
);

public sealed record OrderStep(string Name, DateTimeOffset OccurredAt, string? Detail = null);

public enum OrderStatus
{
    Placed,
    AwaitingPayment,
    Paid,
    Confirmed,
    Shipped,
    Delivered,
    Completed,
    Cancelled,
    Refunding,
    Refunded,
}

public enum PaymentMethod
{
    Wallet,
    Card,
}

public static class OrderScenarios
{
    public const string LostParcel = "lost_parcel";
}

public static class OrderSteps
{
    public const string Placed = "placed";
    public const string StockReserved = "stock_reserved";
    public const string StockReservationFailed = "stock_reservation_failed";
    public const string PaymentRequested = "payment_requested";
    public const string PaymentCaptured = "payment_captured";
    public const string PaymentFailed = "payment_failed";
    public const string StockReleased = "stock_released";
    public const string AuthorizationVoided = "authorization_voided";
    public const string Confirmed = "confirmed";
    public const string Shipped = "shipped";
    public const string StockCommitted = "stock_committed";
    public const string Delivered = "delivered";
    public const string DeliveryFailed = "delivery_failed";
    public const string RefundRequested = "refund_requested";
    public const string Refunded = "refunded";
    public const string Invoiced = "invoiced";
    public const string Cancelled = "cancelled";
}
