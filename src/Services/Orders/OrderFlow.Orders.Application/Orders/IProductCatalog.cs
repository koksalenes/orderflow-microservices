using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Orders.Application.Orders;

public interface IProductCatalog
{
    Task<Result<IReadOnlyList<CatalogProduct>>> GetProductsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken
    );
}

public sealed record CatalogProduct(
    Guid Id,
    string Name,
    decimal Price,
    string Currency,
    Uri? ImageUrl
);
