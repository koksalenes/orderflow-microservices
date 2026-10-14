using FluentValidation;

namespace OrderFlow.Products.Application.Products.GetProductById;

public sealed class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator() =>
        RuleFor(query => query.Id).NotEmpty().WithErrorCode("validation.required");
}
