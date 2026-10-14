namespace OrderFlow.Orders.Application.Orders.GetMyOrders;

public sealed record OrderSummaryDto(
    Guid Id,
    string Number,
    string Status,
    string? CancellationReason,
    decimal Total,
    string Currency,
    string PaymentMethod,
    int ItemCount,
    IReadOnlyList<Uri> Images,
    DateTimeOffset PlacedAt,
    DateTimeOffset UpdatedAt
);
