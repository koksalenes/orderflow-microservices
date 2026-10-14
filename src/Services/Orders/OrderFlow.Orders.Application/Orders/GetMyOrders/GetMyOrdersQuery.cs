using OrderFlow.BuildingBlocks.Application.Paging;

namespace OrderFlow.Orders.Application.Orders.GetMyOrders;

public sealed record GetMyOrdersQuery(
    string CustomerId,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize
);
