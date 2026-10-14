using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Application.Paging;
using OrderFlow.BuildingBlocks.Web.Endpoints;
using OrderFlow.Products.Application.Products.GetProductById;
using OrderFlow.Products.Application.Products.GetProducts;

namespace OrderFlow.Products.Api.Products;

internal static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder group)
    {
        group.AllowAnonymous().WithTags("Products");

        group
            .MapGet("", GetProductsAsync)
            .WithName("GetProducts")
            .WithSummary("Lists the catalog, one page at a time.");

        group
            .MapGet("{id:guid}", GetProductByIdAsync)
            .WithName("GetProductById")
            .WithSummary("Returns one product with its images and specifications.");

        return group;
    }

    private static async Task<
        Results<Ok<PagedResult<ProductSummaryDto>>, ProblemHttpResult>
    > GetProductsAsync(
        IQueryHandler<GetProductsQuery, PagedResult<ProductSummaryDto>> handler,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PageRequest.DefaultPageSize
    ) =>
        (
            await handler.HandleAsync(new GetProductsQuery(page, pageSize), cancellationToken)
        ).ToHttpResult();

    private static async Task<Results<Ok<ProductDto>, ProblemHttpResult>> GetProductByIdAsync(
        Guid id,
        IQueryHandler<GetProductByIdQuery, ProductDto> handler,
        CancellationToken cancellationToken
    ) => (await handler.HandleAsync(new GetProductByIdQuery(id), cancellationToken)).ToHttpResult();
}
