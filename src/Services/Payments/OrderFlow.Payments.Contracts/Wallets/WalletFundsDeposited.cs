namespace OrderFlow.Payments.Contracts.Wallets;

public sealed record WalletFundsDeposited(
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
    string AuthorizationCode,
    DateTimeOffset OccurredAt
)
{
    public const string MessageType = "payments.wallet_funds_deposited.v1";
}
