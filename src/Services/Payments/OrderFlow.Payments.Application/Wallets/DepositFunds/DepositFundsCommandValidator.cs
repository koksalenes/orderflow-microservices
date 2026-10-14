using FluentValidation;
using OrderFlow.Payments.Application.Cards;

namespace OrderFlow.Payments.Application.Wallets.DepositFunds;

public sealed class DepositFundsCommandValidator : AbstractValidator<DepositFundsCommand>
{
    public DepositFundsCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(command => command.OwnerId).NotEmpty().WithErrorCode("validation.required");
        RuleFor(command => command.DepositId).NotEmpty().WithErrorCode("validation.required");
        RuleFor(command => command.Amount)
            .InclusiveBetween(DepositFundsCommand.MinAmount, DepositFundsCommand.MaxAmount)
            .WithErrorCode("validation.range")
            .PrecisionScale(12, 2, ignoreTrailingZeros: true)
            .WithErrorCode("validation.precision");
        RuleFor(command => command.Card)
            .NotNull()
            .WithErrorCode("validation.required")
            .SetValidator(new CardDetailsValidator(timeProvider));
    }
}
