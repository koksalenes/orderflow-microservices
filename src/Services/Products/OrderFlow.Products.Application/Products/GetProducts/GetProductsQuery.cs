using OrderFlow.BuildingBlocks.Application.Paging;

namespace OrderFlow.Products.Application.Products.GetProducts;

public sealed record GetProductsQuery(int Page = 1, int PageSize = PageRequest.DefaultPageSize);
