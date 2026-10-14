using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Inventory.Contracts.Reservations;
using OrderFlow.Orders.Application.Orders.GetOrder;
using OrderFlow.Orders.Contracts.Orders;
using OrderFlow.Orders.Domain.Orders;
using OrderFlow.Payments.Contracts.Orders;

namespace OrderFlow.Orders.Application.Orders.PlaceOrder;

internal sealed class PlaceOrderCommandHandler(
    IProductCatalog catalog,
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    IOutbox outbox,
    TimeProvider timeProvider
) : ICommandHandler<PlaceOrderCommand, OrderDto>
{
    public async Task<Result<OrderDto>> HandleAsync(
        PlaceOrderCommand command,
        CancellationToken cancellationToken
    )
    {
        var products = await catalog
            .GetProductsAsync([.. command.Lines.Select(line => line.ProductId)], cancellationToken)
            .ConfigureAwait(false);
        if (products.IsFailure)
        {
            return Result.Failure<OrderDto>(products.Error!);
        }

        var byId = products.Value.ToDictionary(product => product.Id);
        var missing = command.Lines.FirstOrDefault(line => !byId.ContainsKey(line.ProductId));
        if (missing is not null)
        {
            return Result.Failure<OrderDto>(OrderErrors.ProductUnavailable(missing.ProductId));
        }

        var now = timeProvider.GetUtcNow();
        var address = command.ShippingAddress;
        var order = Order.Place(
            Guid.CreateVersion7(now),
            command.CustomerId,
            command.CustomerEmail,
            command.CustomerName,
            command.Locale,
            command.Lines.Select(line =>
            {
                var product = byId[line.ProductId];
                return new OrderLine(
                    product.Id,
                    product.Name,
                    product.Price,
                    line.Quantity,
                    product.ImageUrl
                );
            }),
            new ShippingAddress(
                address.FullName.Trim(),
                address.Line1.Trim(),
                string.IsNullOrWhiteSpace(address.Line2) ? null : address.Line2.Trim(),
                address.City.Trim(),
                address.PostalCode.Trim(),
                address.Country.Trim(),
                address.Phone.Trim()
            ),
            products.Value[0].Currency,
            command.PaymentMethod == PaymentMethods.Card
                ? PaymentMethod.Card
                : PaymentMethod.Wallet,
            command.CardPaymentId,
            command.CardBrand,
            command.CardLast4,
            command.Scenario,
            now
        );

        orders.Add(order);
        outbox.Enqueue(
            ReserveStock.MessageType,
            new ReserveStock(
                order.Id,
                [.. order.Lines.Select(line => new StockLine(line.ProductId, line.Quantity))]
            )
        );
        outbox.EnqueueStatus(order, OrderStatuses.Placed, now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(OrderDto.From(order));
    }
}
