using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Products.Domain.Products;

public sealed class Product : AggregateRoot<Guid>
{
    private readonly List<ProductSpecification> _specifications = [];

    private Product() { }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string Category { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public IReadOnlyList<string> Images { get; private set; } = [];

    public IReadOnlyCollection<ProductSpecification> Specifications => _specifications;

    public DateTimeOffset CreatedAt { get; private set; }

    public static Product Create(
        Guid id,
        string name,
        string description,
        string category,
        decimal price,
        string currency,
        IEnumerable<string> images,
        IEnumerable<ProductSpecification> specifications,
        DateTimeOffset createdAt
    )
    {
        var product = new Product
        {
            Id = id,
            Name = name,
            Description = description,
            Category = category,
            Price = price,
            Currency = currency,
            Images = [.. images],
            CreatedAt = createdAt,
        };
        product._specifications.AddRange(specifications);

        return product;
    }
}
