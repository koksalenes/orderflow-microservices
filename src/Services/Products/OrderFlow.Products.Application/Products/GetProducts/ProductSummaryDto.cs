namespace OrderFlow.Products.Application.Products.GetProducts;

public sealed record ProductSummaryDto(
    Guid Id,
    string Name,
    string Category,
    decimal Price,
    string Currency,
    Uri? ImageUrl
);
