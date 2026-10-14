using FluentValidation;

namespace OrderFlow.Inventory.Application.Stock.GetStockLevels;

public sealed class GetStockLevelsQueryValidator : AbstractValidator<GetStockLevelsQuery>
{
    public GetStockLevelsQueryValidator()
    {
        RuleFor(query => query.ProductIds)
            .NotEmpty()
            .WithErrorCode("validation.required")
            .Must(ids => ids.Count <= GetStockLevelsQuery.MaxProductIds)
            .WithErrorCode("validation.max_items");
        RuleForEach(query => query.ProductIds).NotEmpty().WithErrorCode("validation.required");
    }
}
