using FluentValidation;

namespace OrderFlow.Products.Application.Products.GetProductsByIds;

public sealed class GetProductsByIdsQueryValidator : AbstractValidator<GetProductsByIdsQuery>
{
    public GetProductsByIdsQueryValidator()
    {
        RuleFor(query => query.Ids)
            .NotEmpty()
            .WithErrorCode("validation.required")
            .Must(ids => ids.Count <= GetProductsByIdsQuery.MaxIds)
            .WithErrorCode("validation.max_items");
        RuleForEach(query => query.Ids).NotEmpty().WithErrorCode("validation.required");
    }
}
