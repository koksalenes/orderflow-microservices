namespace OrderFlow.Orders.Application.Orders.GetOrder;

public sealed record GetOrderQuery(string CustomerId, Guid OrderId);
