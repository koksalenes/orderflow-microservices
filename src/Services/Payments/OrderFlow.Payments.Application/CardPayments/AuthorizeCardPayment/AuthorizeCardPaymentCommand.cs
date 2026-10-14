using OrderFlow.Payments.Application.Cards;

namespace OrderFlow.Payments.Application.CardPayments.AuthorizeCardPayment;

public sealed record AuthorizeCardPaymentCommand(string OwnerId, decimal Amount, CardDetails Card)
{
    public const decimal MaxAmount = 100_000m;
}
