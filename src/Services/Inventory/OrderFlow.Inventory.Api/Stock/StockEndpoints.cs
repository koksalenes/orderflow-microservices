using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Web.Endpoints;
using OrderFlow.Inventory.Application.Stock;
using OrderFlow.Inventory.Application.Stock.GetStockLevel;
using OrderFlow.Inventory.Application.Stock.GetStockLevels;

namespace OrderFlow.Inventory.Api.Stock;

internal static class StockEndpoints
{
    public static RouteGroupBuilder MapStockEndpoints(this RouteGroupBuilder group)
    {
        var stock = group.MapGroup("stock").AllowAnonymous().WithTags("Stock");

        stock
            .MapGet("", GetStockLevelsAsync)
            .WithName("GetStockLevels")
            .WithSummary(
                "Returns the available quantity of several products (?productId=…&productId=…)."
            );

        stock
            .MapGet("{productId:guid}", GetStockLevelAsync)
            .WithName("GetStockLevel")
            .WithSummary("Returns the available quantity of one product.");

        return group;
    }

    private static async Task<
        Results<Ok<IReadOnlyList<StockLevelDto>>, ProblemHttpResult>
    > GetStockLevelsAsync(
        IQueryHandler<GetStockLevelsQuery, IReadOnlyList<StockLevelDto>> handler,
        CancellationToken cancellationToken,
        [FromQuery(Name = "productId")] Guid[]? productIds = null
    ) =>
        (
            await handler.HandleAsync(new GetStockLevelsQuery(productIds ?? []), cancellationToken)
        ).ToHttpResult();

    private static async Task<Results<Ok<StockLevelDto>, ProblemHttpResult>> GetStockLevelAsync(
        Guid productId,
        IQueryHandler<GetStockLevelQuery, StockLevelDto> handler,
        CancellationToken cancellationToken
    ) =>
        (
            await handler.HandleAsync(new GetStockLevelQuery(productId), cancellationToken)
        ).ToHttpResult();
}
