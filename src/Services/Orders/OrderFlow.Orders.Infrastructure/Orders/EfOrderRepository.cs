using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Application.Orders;
using OrderFlow.Orders.Domain.Orders;
using OrderFlow.Orders.Infrastructure.Persistence;

namespace OrderFlow.Orders.Infrastructure.Orders;

internal sealed class EfOrderRepository(OrdersDbContext context) : IOrderRepository
{
    public Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken) =>
        context.Orders.SingleOrDefaultAsync(order => order.Id == orderId, cancellationToken);

    public void Add(Order order) => context.Orders.Add(order);

    public async Task<IReadOnlyList<Guid>> GetDueAsync(
        DateTimeOffset now,
        int limit,
        CancellationToken cancellationToken
    ) =>
        await context
            .Orders.AsNoTracking()
            .Where(order => order.NextActionAt != null && order.NextActionAt <= now)
            .OrderBy(order => order.NextActionAt)
            .Select(order => order.Id)
            .Take(limit)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
