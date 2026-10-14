using FluentValidation;

namespace OrderFlow.Payments.Application.Deposits.GetDepositSagas;

public sealed class GetDepositSagasQueryValidator : AbstractValidator<GetDepositSagasQuery>
{
    public GetDepositSagasQueryValidator() =>
        RuleFor(query => query.Limit)
            .InclusiveBetween(1, GetDepositSagasQuery.MaxLimit)
            .WithErrorCode("validation.range");
}
