using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Orders.Domain.Orders;

public static class OrderErrors
{
    public static Error NotFound(Guid orderId) =>
        Error.NotFound(
            "orders.not_found",
            $"Order '{orderId}' was not found.",
            new Dictionary<string, object?>(StringComparer.Ordinal) { ["orderId"] = orderId }
        );

    public static Error ProductUnavailable(Guid productId) =>
        new(
            "orders.product_unavailable",
            $"Product '{productId}' is not in the catalog.",
            ErrorType.Conflict,
            new Dictionary<string, object?>(StringComparer.Ordinal) { ["productId"] = productId }
        );

    public static Error CatalogUnavailable() =>
        Error.Failure("orders.catalog_unavailable", "The product catalog could not be reached.");
}
