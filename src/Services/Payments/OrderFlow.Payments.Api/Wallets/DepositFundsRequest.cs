namespace OrderFlow.Payments.Api.Wallets;

/** Body of POST /wallet/deposits. DepositId is created by the checkout page, so a retried request is not stored twice. */
internal sealed record DepositFundsRequest(
    Guid DepositId,
    decimal Amount,
    CardRequest Card,
    string? Locale = null
);

internal sealed record CardRequest(
    string HolderName,
    string Number,
    int ExpiryMonth,
    int ExpiryYear,
    string Cvv
)
{
    /** Never print the card number or the CVV. */
    public override string ToString() => "CardRequest { *** }";
}
