using System.Globalization;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Orders.Application.Orders;
using OrderFlow.Orders.Domain.Orders;
using OrderFlow.Products.Contracts.Grpc;
using CatalogProduct = OrderFlow.Orders.Application.Orders.CatalogProduct;

namespace OrderFlow.Orders.Infrastructure.Catalog;

internal sealed partial class GrpcProductCatalog(
    ProductCatalog.ProductCatalogClient client,
    ILogger<GrpcProductCatalog> logger
) : IProductCatalog
{
    public async Task<Result<IReadOnlyList<CatalogProduct>>> GetProductsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken
    )
    {
        var request = new GetProductsRequest();
        request.ProductIds.AddRange(productIds.Select(id => id.ToString()));

        try
        {
            var response = await client
                .GetProductsAsync(request, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return Result.Success<IReadOnlyList<CatalogProduct>>([
                .. response.Products.Select(product => new CatalogProduct(
                    Guid.Parse(product.Id),
                    product.Name,
                    decimal.Parse(product.Price, CultureInfo.InvariantCulture),
                    product.Currency,
                    string.IsNullOrEmpty(product.ImageUrl) ? null : new Uri(product.ImageUrl)
                )),
            ]);
        }
        catch (RpcException exception)
        {
            LogCatalogUnavailable(exception, exception.StatusCode);
            return Result.Failure<IReadOnlyList<CatalogProduct>>(OrderErrors.CatalogUnavailable());
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Product catalog gRPC call failed with {StatusCode}"
    )]
    private partial void LogCatalogUnavailable(Exception exception, StatusCode statusCode);
}
