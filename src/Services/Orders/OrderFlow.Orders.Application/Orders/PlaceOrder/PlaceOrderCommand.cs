namespace OrderFlow.Orders.Application.Orders.PlaceOrder;

public sealed record PlaceOrderCommand(
    string CustomerId,
    string? CustomerEmail,
    string? CustomerName,
    string Locale,
    IReadOnlyList<PlaceOrderLine> Lines,
    PlaceOrderAddress ShippingAddress,
    string PaymentMethod,
    Guid? CardPaymentId,
    string? CardBrand,
    string? CardLast4,
    string? Scenario
)
{
    public const int MaxLines = 20;
    public const int MaxQuantity = 10;
}

public sealed record PlaceOrderLine(Guid ProductId, int Quantity);

public sealed record PlaceOrderAddress(
    string FullName,
    string Line1,
    string? Line2,
    string City,
    string PostalCode,
    string Country,
    string Phone
);
