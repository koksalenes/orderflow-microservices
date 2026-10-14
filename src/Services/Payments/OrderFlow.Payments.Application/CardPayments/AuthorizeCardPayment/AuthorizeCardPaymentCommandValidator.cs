using FluentValidation;
using OrderFlow.Payments.Application.Cards;

namespace OrderFlow.Payments.Application.CardPayments.AuthorizeCardPayment;

public sealed class AuthorizeCardPaymentCommandValidator
    : AbstractValidator<AuthorizeCardPaymentCommand>
{
    public AuthorizeCardPaymentCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(command => command.OwnerId).NotEmpty().WithErrorCode("validation.required");
        RuleFor(command => command.Amount)
            .GreaterThan(0)
            .WithErrorCode("validation.range")
            .LessThanOrEqualTo(AuthorizeCardPaymentCommand.MaxAmount)
            .WithErrorCode("validation.range")
            .PrecisionScale(12, 2, ignoreTrailingZeros: true)
            .WithErrorCode("validation.precision");
        RuleFor(command => command.Card)
            .NotNull()
            .WithErrorCode("validation.required")
            .SetValidator(new CardDetailsValidator(timeProvider));
    }
}
