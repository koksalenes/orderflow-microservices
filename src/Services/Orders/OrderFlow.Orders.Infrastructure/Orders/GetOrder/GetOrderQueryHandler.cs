using System.Text.Json;
using Dapper;
using Npgsql;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Orders.Application.Orders.GetOrder;
using OrderFlow.Orders.Domain.Orders;
using OrderFlow.Payments.Contracts.Orders;

namespace OrderFlow.Orders.Infrastructure.Orders.GetOrder;

internal sealed class GetOrderQueryHandler(NpgsqlDataSource dataSource)
    : IQueryHandler<GetOrderQuery, OrderDto>
{
    private const string Sql = """
        SELECT id AS Id, number AS Number, status AS Status, cancellation_reason AS CancellationReason,
               lines::text AS Lines, subtotal AS Subtotal, shipping_fee AS ShippingFee, total AS Total,
               currency AS Currency, payment_method AS PaymentMethod, card_brand AS CardBrand,
               card_last4 AS CardLast4, shipping_address::text AS ShippingAddress,
               tracking_number AS TrackingNumber, invoice_number AS InvoiceNumber, scenario AS Scenario,
               placed_at AS PlacedAt, updated_at AS UpdatedAt, next_action_at AS NextActionAt,
               steps::text AS Steps
        FROM orders
        WHERE id = @OrderId AND customer_id = @CustomerId;
        """;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<OrderDto>> HandleAsync(
        GetOrderQuery query,
        CancellationToken cancellationToken
    )
    {
        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var row = await connection
                .QuerySingleOrDefaultAsync<OrderRow>(
                    new CommandDefinition(
                        Sql,
                        new { query.OrderId, query.CustomerId },
                        cancellationToken: cancellationToken
                    )
                )
                .ConfigureAwait(false);

            return row is null
                ? Result.Failure<OrderDto>(OrderErrors.NotFound(query.OrderId))
                : Result.Success(row.ToDto());
        }
    }

    internal sealed class OrderRow
    {
        public Guid Id { get; init; }

        public string Number { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public string? CancellationReason { get; init; }

        public string Lines { get; init; } = "[]";

        public decimal Subtotal { get; init; }

        public decimal ShippingFee { get; init; }

        public decimal Total { get; init; }

        public string Currency { get; init; } = string.Empty;

        public string PaymentMethod { get; init; } = string.Empty;

        public string? CardBrand { get; init; }

        public string? CardLast4 { get; init; }

        public string ShippingAddress { get; init; } = "{}";

        public string? TrackingNumber { get; init; }

        public string? InvoiceNumber { get; init; }

        public string? Scenario { get; init; }

        public DateTimeOffset PlacedAt { get; init; }

        public DateTimeOffset UpdatedAt { get; init; }

        public DateTimeOffset? NextActionAt { get; init; }

        public string Steps { get; init; } = "[]";

        public OrderDto ToDto()
        {
            var lines = JsonSerializer.Deserialize<List<LineJson>>(Lines, JsonOptions) ?? [];
            return new(
                Id,
                Number,
                Status,
                CancellationReason,
                [
                    .. lines.Select(line => new OrderLineDto(
                        line.ProductId,
                        line.Name,
                        line.UnitPrice,
                        line.Quantity,
                        line.UnitPrice * line.Quantity,
                        line.ImageUrl
                    )),
                ],
                Subtotal,
                ShippingFee,
                Total,
                Currency,
                PaymentMethod == nameof(Domain.Orders.PaymentMethod.Card)
                    ? PaymentMethods.Card
                    : PaymentMethods.Wallet,
                CardBrand,
                CardLast4,
                JsonSerializer.Deserialize<OrderAddressDto>(ShippingAddress, JsonOptions)!,
                TrackingNumber,
                InvoiceNumber,
                Scenario,
                PlacedAt,
                UpdatedAt,
                NextActionAt,
                JsonSerializer.Deserialize<List<OrderStepDto>>(Steps, JsonOptions) ?? []
            );
        }
    }

    internal sealed record LineJson(
        Guid ProductId,
        string Name,
        decimal UnitPrice,
        int Quantity,
        Uri? ImageUrl
    );
}
