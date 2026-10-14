using System.Text.Json;
using Dapper;
using Npgsql;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Products.Application.Products.GetProductById;
using OrderFlow.Products.Domain.Products;

namespace OrderFlow.Products.Infrastructure.Products.GetProductById;

internal sealed class GetProductByIdQueryHandler(NpgsqlDataSource dataSource)
    : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    private const string Sql = """
        SELECT id AS Id, name AS Name, description AS Description, category AS Category,
               price AS Price, currency AS Currency, images AS Images,
               specifications::text AS Specifications
        FROM products
        WHERE id = @Id;
        """;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<ProductDto>> HandleAsync(
        GetProductByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var row = await connection
                .QuerySingleOrDefaultAsync<ProductRow>(
                    new CommandDefinition(
                        Sql,
                        new { query.Id },
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);

            return row is null
                ? Result.Failure<ProductDto>(ProductErrors.NotFound(query.Id))
                : Result.Success(row.ToDto());
        }
    }

    internal sealed class ProductRow
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string Category { get; init; } = string.Empty;

        public decimal Price { get; init; }

        public string Currency { get; init; } = string.Empty;

        public string[] Images { get; init; } = [];

        public string? Specifications { get; init; }

        public ProductDto ToDto() =>
            new(
                Id,
                Name,
                Description,
                Category,
                Price,
                Currency,
                [.. Images.Select(image => new Uri(image))],
                Specifications is null
                    ? []
                    : JsonSerializer.Deserialize<List<ProductSpecificationDto>>(
                        Specifications,
                        JsonOptions
                    ) ?? []
            );
    }
}
