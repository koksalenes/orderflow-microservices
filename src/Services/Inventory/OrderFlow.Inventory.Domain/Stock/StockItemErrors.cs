using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Inventory.Domain.Stock;

public static class StockItemErrors
{
    public static Error NotFound(Guid productId) =>
        Error.NotFound(
            "inventory.stock_not_found",
            $"No stock record exists for product '{productId}'.",
            new Dictionary<string, object?>(StringComparer.Ordinal) { ["productId"] = productId }
        );
}
