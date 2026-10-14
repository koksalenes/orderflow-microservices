namespace OrderFlow.Products.Application.Products.GetProductsByIds;

public sealed record CatalogProductDto(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    Uri? ImageUrl
);
