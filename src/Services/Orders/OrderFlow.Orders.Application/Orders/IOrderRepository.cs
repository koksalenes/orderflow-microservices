using OrderFlow.Orders.Domain.Orders;

namespace OrderFlow.Orders.Application.Orders;

public interface IOrderRepository
{
    Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken);

    void Add(Order order);

    Task<IReadOnlyList<Guid>> GetDueAsync(
        DateTimeOffset now,
        int limit,
        CancellationToken cancellationToken
    );
}
