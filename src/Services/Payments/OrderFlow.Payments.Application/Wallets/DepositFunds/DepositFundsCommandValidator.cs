using FluentValidation;
using OrderFlow.Payments.Application.Cards;

namespace OrderFlow.Payments.Application.Wallets.DepositFunds;

/**
 * Test checkout rules: any well-formed card number is accepted, as long as the card has not expired
 * and the CVV has the right length. There is no Luhn check, so testers can type any digits.
 */
public sealed class DepositFundsCommandValidator : AbstractValidator<DepositFundsCommand>
{
    private const int MaxYearsAhead = 20;

    public DepositFundsCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(command => command.OwnerId).NotEmpty().WithErrorCode("validation.required");
        RuleFor(command => command.DepositId).NotEmpty().WithErrorCode("validation.required");
        RuleFor(command => command.Amount)
            .InclusiveBetween(DepositFundsCommand.MinAmount, DepositFundsCommand.MaxAmount)
            .WithErrorCode("validation.range")
            .PrecisionScale(12, 2, ignoreTrailingZeros: true)
            .WithErrorCode("validation.precision");

        RuleFor(command => command.Card).NotNull().WithErrorCode("validation.required");
        RuleFor(command => command.Card.HolderName)
            .NotEmpty()
            .WithErrorCode("validation.required")
            .Length(2, 100)
            .WithErrorCode("validation.length")
            .When(command => command.Card is not null);
        RuleFor(command => command.Card.Number)
            .Matches(@"^\d{12,19}$")
            .WithErrorCode("validation.card_number")
            .When(command => command.Card is not null);
        RuleFor(command => command.Card.ExpiryMonth)
            .InclusiveBetween(1, 12)
            .WithErrorCode("validation.card_expiry")
            .When(command => command.Card is not null);
        RuleFor(command => command.Card)
            .Must(card => !IsExpired(card, timeProvider.GetUtcNow()))
            .OverridePropertyName("Card.Expiry")
            .WithErrorCode("validation.card_expired")
            .Must(card => card.ExpiryYear <= timeProvider.GetUtcNow().Year + MaxYearsAhead)
            .OverridePropertyName("Card.Expiry")
            .WithErrorCode("validation.card_expiry")
            .When(command => command.Card is { ExpiryMonth: >= 1 and <= 12 });
        RuleFor(command => command.Card.Cvv)
            .Must(
                (command, cvv) =>
                    cvv is not null
                    && cvv.Length == CardBrands.CvvLength(command.Card.Number ?? string.Empty)
                    && cvv.All(char.IsAsciiDigit)
            )
            .WithErrorCode("validation.card_cvv")
            .When(command => command.Card is not null);
    }

    private static bool IsExpired(CardDetails card, DateTimeOffset now) =>
        card.ExpiryYear < now.Year || (card.ExpiryYear == now.Year && card.ExpiryMonth < now.Month);
}
