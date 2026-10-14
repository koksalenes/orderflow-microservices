using OrderFlow.BuildingBlocks.Application.Paging;

namespace OrderFlow.Products.Application.Products.GetProducts;

public sealed record GetProductsQuery(
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize,
    string? Search = null
)
{
    public const int MaxSearchLength = 100;
    public const int MaxSearchTerms = 5;

    public IReadOnlyList<string> SearchTerms =>
        Search?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        ?? [];
}
