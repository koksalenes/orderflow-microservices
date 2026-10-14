using Dapper;
using Npgsql;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Inventory.Application.Stock;
using OrderFlow.Inventory.Application.Stock.GetStockLevel;
using OrderFlow.Inventory.Domain.Stock;

namespace OrderFlow.Inventory.Infrastructure.Stock.GetStockLevel;

internal sealed class GetStockLevelQueryHandler(NpgsqlDataSource dataSource)
    : IQueryHandler<GetStockLevelQuery, StockLevelDto>
{
    private const string Sql = """
        SELECT product_id AS ProductId, on_hand - reserved AS Available
        FROM stock_items
        WHERE product_id = @ProductId;
        """;

    public async Task<Result<StockLevelDto>> HandleAsync(
        GetStockLevelQuery query,
        CancellationToken cancellationToken
    )
    {
        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var level = await connection
                .QuerySingleOrDefaultAsync<StockLevelDto>(
                    new CommandDefinition(
                        Sql,
                        new { query.ProductId },
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);

            return level is null
                ? Result.Failure<StockLevelDto>(StockItemErrors.NotFound(query.ProductId))
                : Result.Success(level);
        }
    }
}
