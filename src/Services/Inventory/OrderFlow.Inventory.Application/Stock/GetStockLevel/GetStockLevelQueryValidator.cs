using FluentValidation;

namespace OrderFlow.Inventory.Application.Stock.GetStockLevel;

public sealed class GetStockLevelQueryValidator : AbstractValidator<GetStockLevelQuery>
{
    public GetStockLevelQueryValidator() =>
        RuleFor(query => query.ProductId).NotEmpty().WithErrorCode("validation.required");
}
