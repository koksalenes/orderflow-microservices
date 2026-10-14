using FluentValidation;

namespace OrderFlow.Payments.Application.Cards;

public sealed class CardDetailsValidator : AbstractValidator<CardDetails>
{
    private const int MaxYearsAhead = 20;

    public CardDetailsValidator(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        RuleFor(card => card.HolderName)
            .NotEmpty()
            .WithErrorCode("validation.required")
            .Length(2, 100)
            .WithErrorCode("validation.length");
        RuleFor(card => card.Number)
            .Matches(@"^\d{12,19}$")
            .WithErrorCode("validation.card_number");
        RuleFor(card => card.ExpiryMonth)
            .InclusiveBetween(1, 12)
            .WithErrorCode("validation.card_expiry");
        RuleFor(card => card)
            .Must(card => !IsExpired(card, timeProvider.GetUtcNow()))
            .OverridePropertyName("Expiry")
            .WithErrorCode("validation.card_expired")
            .Must(card => card.ExpiryYear <= timeProvider.GetUtcNow().Year + MaxYearsAhead)
            .OverridePropertyName("Expiry")
            .WithErrorCode("validation.card_expiry")
            .When(card => card.ExpiryMonth is >= 1 and <= 12);
        RuleFor(card => card.Cvv)
            .Must(
                (card, cvv) =>
                    cvv is not null
                    && cvv.Length == CardBrands.CvvLength(card.Number ?? string.Empty)
                    && cvv.All(char.IsAsciiDigit)
            )
            .WithErrorCode("validation.card_cvv");
    }

    private static bool IsExpired(CardDetails card, DateTimeOffset now) =>
        card.ExpiryYear < now.Year || (card.ExpiryYear == now.Year && card.ExpiryMonth < now.Month);
}
