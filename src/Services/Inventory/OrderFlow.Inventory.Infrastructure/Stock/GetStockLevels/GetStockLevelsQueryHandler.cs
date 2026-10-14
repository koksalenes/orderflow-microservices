using Dapper;
using Npgsql;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Inventory.Application.Stock;
using OrderFlow.Inventory.Application.Stock.GetStockLevels;

namespace OrderFlow.Inventory.Infrastructure.Stock.GetStockLevels;

internal sealed class GetStockLevelsQueryHandler(NpgsqlDataSource dataSource)
    : IQueryHandler<GetStockLevelsQuery, IReadOnlyList<StockLevelDto>>
{
    private const string Sql = """
        SELECT product_id AS ProductId, on_hand - reserved AS Available
        FROM stock_items
        WHERE product_id = ANY(@ProductIds);
        """;

    public async Task<Result<IReadOnlyList<StockLevelDto>>> HandleAsync(
        GetStockLevelsQuery query,
        CancellationToken cancellationToken
    )
    {
        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var levels = await connection
                .QueryAsync<StockLevelDto>(
                    new CommandDefinition(
                        Sql,
                        new { ProductIds = query.ProductIds.Distinct().ToArray() },
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);

            return Result.Success<IReadOnlyList<StockLevelDto>>([.. levels]);
        }
    }
}
