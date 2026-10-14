using OrderFlow.Payments.Contracts.Wallets;

namespace OrderFlow.Notifications.Application.Deposits;

internal sealed record WalletEmailModel(
    WalletEmailKind Kind,
    Guid DepositId,
    string RecipientEmail,
    string? RecipientName,
    string Locale,
    decimal Amount,
    string Currency,
    decimal Balance,
    string CardBrand,
    string CardLast4,
    string Code,
    DateTimeOffset OccurredAt
)
{
    public static WalletEmailModel? From(WalletFundsDeposited message) =>
        message.RecipientEmail is { Length: > 0 } email
            ? new(
                WalletEmailKind.Deposit,
                message.DepositId,
                email,
                message.RecipientName,
                message.Locale,
                message.Amount,
                message.Currency,
                message.Balance,
                message.CardBrand,
                message.CardLast4,
                message.AuthorizationCode,
                message.OccurredAt
            )
            : null;

    public static WalletEmailModel? From(WalletDepositReversed message) =>
        message.RecipientEmail is { Length: > 0 } email
            ? new(
                WalletEmailKind.Reversal,
                message.DepositId,
                email,
                message.RecipientName,
                message.Locale,
                message.Amount,
                message.Currency,
                message.Balance,
                message.CardBrand,
                message.CardLast4,
                message.ReasonCode,
                message.OccurredAt
            )
            : null;
}
