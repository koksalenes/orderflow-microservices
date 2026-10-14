using Dapper;
using Npgsql;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Products.Application.Products.GetProductsByIds;

namespace OrderFlow.Products.Infrastructure.Products.GetProductsByIds;

internal sealed class GetProductsByIdsQueryHandler(NpgsqlDataSource dataSource)
    : IQueryHandler<GetProductsByIdsQuery, IReadOnlyList<CatalogProductDto>>
{
    private const string Sql = """
        SELECT id AS Id, name AS Name, price AS Price, currency AS Currency, images[1] AS ImageUrl
        FROM products
        WHERE id = ANY(@Ids);
        """;

    public async Task<Result<IReadOnlyList<CatalogProductDto>>> HandleAsync(
        GetProductsByIdsQuery query,
        CancellationToken cancellationToken
    )
    {
        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var rows = await connection
                .QueryAsync<CatalogProductRow>(
                    new CommandDefinition(
                        Sql,
                        new { Ids = query.Ids.Distinct().ToArray() },
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);

            return Result.Success<IReadOnlyList<CatalogProductDto>>([
                .. rows.Select(row => row.ToDto()),
            ]);
        }
    }

    internal sealed class CatalogProductRow
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public decimal Price { get; init; }

        public string Currency { get; init; } = string.Empty;

        public string? ImageUrl { get; init; }

        public CatalogProductDto ToDto() =>
            new(Id, Name, Price, Currency, ImageUrl is null ? null : new Uri(ImageUrl));
    }
}
