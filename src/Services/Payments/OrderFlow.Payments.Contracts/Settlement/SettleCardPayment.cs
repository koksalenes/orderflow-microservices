namespace OrderFlow.Payments.Contracts.Settlement;

public sealed record SettleCardPayment(
    Guid DepositId,
    string AuthorizationCode,
    decimal Amount,
    string Currency,
    string CardBrand,
    string CardLast4
)
{
    public const string MessageType = "payments.settle_card_payment.v1";
}
