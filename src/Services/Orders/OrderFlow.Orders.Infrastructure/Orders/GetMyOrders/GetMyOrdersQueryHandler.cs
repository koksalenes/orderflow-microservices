using System.Text.Json;
using Dapper;
using Npgsql;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Application.Paging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Orders.Application.Orders.GetMyOrders;
using OrderFlow.Payments.Contracts.Orders;

namespace OrderFlow.Orders.Infrastructure.Orders.GetMyOrders;

internal sealed class GetMyOrdersQueryHandler(NpgsqlDataSource dataSource)
    : IQueryHandler<GetMyOrdersQuery, PagedResult<OrderSummaryDto>>
{
    private const string Sql = """
        SELECT id AS Id, number AS Number, status AS Status, cancellation_reason AS CancellationReason,
               total AS Total, currency AS Currency, payment_method AS PaymentMethod,
               lines::text AS Lines, placed_at AS PlacedAt, updated_at AS UpdatedAt
        FROM orders
        WHERE customer_id = @CustomerId
        ORDER BY placed_at DESC
        LIMIT @PageSize OFFSET @Offset;

        SELECT count(*)::int FROM orders WHERE customer_id = @CustomerId;
        """;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<PagedResult<OrderSummaryDto>>> HandleAsync(
        GetMyOrdersQuery query,
        CancellationToken cancellationToken
    )
    {
        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var reader = await connection
                .QueryMultipleAsync(
                    new CommandDefinition(
                        Sql,
                        new
                        {
                            query.CustomerId,
                            query.PageSize,
                            Offset = PageRequest.Offset(query.Page, query.PageSize),
                        },
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);
            await using (reader.ConfigureAwait(false))
            {
                var rows = await reader.ReadAsync<SummaryRow>().ConfigureAwait(false);
                var total = await reader.ReadSingleAsync<int>().ConfigureAwait(false);

                return Result.Success(
                    new PagedResult<OrderSummaryDto>(
                        [.. rows.Select(row => row.ToDto())],
                        query.Page,
                        query.PageSize,
                        total
                    )
                );
            }
        }
    }

    internal sealed class SummaryRow
    {
        public Guid Id { get; init; }

        public string Number { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public string? CancellationReason { get; init; }

        public decimal Total { get; init; }

        public string Currency { get; init; } = string.Empty;

        public string PaymentMethod { get; init; } = string.Empty;

        public string Lines { get; init; } = "[]";

        public DateTimeOffset PlacedAt { get; init; }

        public DateTimeOffset UpdatedAt { get; init; }

        public OrderSummaryDto ToDto()
        {
            var lines = JsonSerializer.Deserialize<List<LineJson>>(Lines, JsonOptions) ?? [];
            return new(
                Id,
                Number,
                Status,
                CancellationReason,
                Total,
                Currency,
                PaymentMethod == "Card" ? PaymentMethods.Card : PaymentMethods.Wallet,
                lines.Sum(line => line.Quantity),
                [.. lines.Select(line => line.ImageUrl).OfType<Uri>().Take(4)],
                PlacedAt,
                UpdatedAt
            );
        }
    }

    internal sealed record LineJson(int Quantity, Uri? ImageUrl);
}
