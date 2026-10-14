namespace OrderFlow.Payments.Contracts.Wallets;

public sealed record WalletDepositReversed(
    Guid DepositId,
    string OwnerId,
    string? RecipientEmail,
    string? RecipientName,
    string Locale,
    decimal Amount,
    string Currency,
    decimal Balance,
    string CardBrand,
    string CardLast4,
    string ReasonCode,
    DateTimeOffset OccurredAt
)
{
    public const string MessageType = "payments.wallet_deposit_reversed.v1";
}
