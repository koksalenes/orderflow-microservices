using System.Globalization;
using Grpc.Core;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Products.Application.Products.GetProductsByIds;
using OrderFlow.Products.Contracts.Grpc;

namespace OrderFlow.Products.Api.Grpc;

internal sealed class ProductCatalogService(
    IQueryHandler<GetProductsByIdsQuery, IReadOnlyList<CatalogProductDto>> handler
) : ProductCatalog.ProductCatalogBase
{
    public override async Task<GetProductsResponse> GetProducts(
        GetProductsRequest request,
        ServerCallContext context
    )
    {
        var ids = new List<Guid>();
        foreach (var value in request.ProductIds)
        {
            if (!Guid.TryParse(value, out var id))
            {
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument, $"'{value}' is not a product id.")
                );
            }

            ids.Add(id);
        }

        var result = await handler
            .HandleAsync(new GetProductsByIdsQuery(ids), context.CancellationToken)
            .ConfigureAwait(false);
        if (result.IsFailure)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, result.Error!.Code));
        }

        var response = new GetProductsResponse();
        response.Products.AddRange(
            result.Value.Select(product => new CatalogProduct
            {
                Id = product.Id.ToString(),
                Name = product.Name,
                Price = product.Price.ToString(CultureInfo.InvariantCulture),
                Currency = product.Currency,
                ImageUrl = product.ImageUrl?.ToString() ?? string.Empty,
            })
        );
        return response;
    }
}
