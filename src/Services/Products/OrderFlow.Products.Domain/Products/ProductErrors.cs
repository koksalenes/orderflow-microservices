using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Products.Domain.Products;

public static class ProductErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound(
            "products.not_found",
            $"Product '{id}' was not found.",
            new Dictionary<string, object?>(StringComparer.Ordinal) { ["id"] = id }
        );
}
