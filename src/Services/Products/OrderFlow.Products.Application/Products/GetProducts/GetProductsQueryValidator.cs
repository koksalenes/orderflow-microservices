using FluentValidation;
using OrderFlow.BuildingBlocks.Application.Paging;

namespace OrderFlow.Products.Application.Products.GetProducts;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1).WithErrorCode("validation.min");
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, PageRequest.MaxPageSize)
            .WithErrorCode("validation.range");
        RuleFor(query => query.Search)
            .MaximumLength(GetProductsQuery.MaxSearchLength)
            .WithErrorCode("validation.max_length");
        RuleFor(query => query.SearchTerms.Count)
            .LessThanOrEqualTo(GetProductsQuery.MaxSearchTerms)
            .OverridePropertyName("Search")
            .WithErrorCode("validation.max_items");
    }
}
