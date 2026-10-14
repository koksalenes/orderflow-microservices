using Dapper;
using Npgsql;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Application.Paging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Products.Application.Products.GetProducts;

namespace OrderFlow.Products.Infrastructure.Products.GetProducts;

internal sealed class GetProductsQueryHandler(NpgsqlDataSource dataSource)
    : IQueryHandler<GetProductsQuery, PagedResult<ProductSummaryDto>>
{
    private const string Filter = """
        NOT EXISTS (
            SELECT 1 FROM unnest(@Patterns::text[]) AS pattern
            WHERE NOT (
                p.name ILIKE pattern
                OR p.category ILIKE pattern
                OR p.description ILIKE pattern
                OR EXISTS (
                    SELECT 1 FROM jsonb_array_elements(coalesce(p.specifications, '[]')) AS spec
                    WHERE spec->>'Value' ILIKE pattern
                )
            )
        )
        """;

    private const string Sql = $"""
        SELECT id AS Id, name AS Name, category AS Category, price AS Price,
               currency AS Currency, images[1] AS ImageUrl
        FROM products p
        WHERE {Filter}
        ORDER BY (p.name ILIKE @NamePattern) DESC, id
        LIMIT @PageSize OFFSET @Offset;

        SELECT count(*)::int FROM products p WHERE {Filter};
        """;

    public async Task<Result<PagedResult<ProductSummaryDto>>> HandleAsync(
        GetProductsQuery query,
        CancellationToken cancellationToken
    )
    {
        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var command = new CommandDefinition(
                Sql,
                new
                {
                    query.PageSize,
                    Offset = PageRequest.Offset(query.Page, query.PageSize),
                    Patterns = query.SearchTerms.Select(ContainsPattern).ToArray(),
                    NamePattern = query.SearchTerms.Count == 0
                        ? "%"
                        : ContainsPattern(string.Join(' ', query.SearchTerms)),
                },
                cancellationToken: cancellationToken
            );
            var reader = await connection.QueryMultipleAsync(command).ConfigureAwait(false);
            await using (reader.ConfigureAwait(false))
            {
                var rows = await reader.ReadAsync<ProductSummaryRow>().ConfigureAwait(false);
                var totalCount = await reader.ReadSingleAsync<int>().ConfigureAwait(false);

                return Result.Success(
                    new PagedResult<ProductSummaryDto>(
                        [.. rows.Select(row => row.ToDto())],
                        query.Page,
                        query.PageSize,
                        totalCount
                    )
                );
            }
        }
    }

    private static string ContainsPattern(string term) =>
        $"%{term.Replace(@"\", @"\\", StringComparison.Ordinal).Replace("%", @"\%", StringComparison.Ordinal).Replace("_", @"\_", StringComparison.Ordinal)}%";

    internal sealed class ProductSummaryRow
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string Category { get; init; } = string.Empty;

        public decimal Price { get; init; }

        public string Currency { get; init; } = string.Empty;

        public string? ImageUrl { get; init; }

        public ProductSummaryDto ToDto() =>
            new(Id, Name, Category, Price, Currency, ImageUrl is null ? null : new Uri(ImageUrl));
    }
}
