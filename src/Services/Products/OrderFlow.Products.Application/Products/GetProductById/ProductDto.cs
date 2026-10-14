namespace OrderFlow.Products.Application.Products.GetProductById;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    string Category,
    decimal Price,
    string Currency,
    IReadOnlyList<Uri> Images,
    IReadOnlyList<ProductSpecificationDto> Specifications
);

public sealed record ProductSpecificationDto(string Name, string Value);
