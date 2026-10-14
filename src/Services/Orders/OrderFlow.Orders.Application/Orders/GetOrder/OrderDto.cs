using OrderFlow.Orders.Domain.Orders;

namespace OrderFlow.Orders.Application.Orders.GetOrder;

public sealed record OrderDto(
    Guid Id,
    string Number,
    string Status,
    string? CancellationReason,
    IReadOnlyList<OrderLineDto> Lines,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Total,
    string Currency,
    string PaymentMethod,
    string? CardBrand,
    string? CardLast4,
    OrderAddressDto ShippingAddress,
    string? TrackingNumber,
    string? InvoiceNumber,
    string? Scenario,
    DateTimeOffset PlacedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? NextActionAt,
    IReadOnlyList<OrderStepDto> Steps
)
{
    public static OrderDto From(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new(
            order.Id,
            order.Number,
            order.Status.ToString(),
            order.CancellationReason,
            [
                .. order.Lines.Select(line => new OrderLineDto(
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
            new OrderAddressDto(
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
            order.Scenario,
            order.PlacedAt,
            order.UpdatedAt,
            order.NextActionAt,
            [
                .. order.Steps.Select(step => new OrderStepDto(
                    step.Name,
                    step.OccurredAt,
                    step.Detail
                )),
            ]
        );
    }
}

public sealed record OrderLineDto(
    Guid ProductId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    Uri? ImageUrl
);

public sealed record OrderAddressDto(
    string FullName,
    string Line1,
    string? Line2,
    string City,
    string PostalCode,
    string Country,
    string Phone
);

public sealed record OrderStepDto(string Name, DateTimeOffset OccurredAt, string? Detail);
