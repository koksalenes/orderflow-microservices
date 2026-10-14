namespace OrderFlow.Payments.Application.Wallets.GetWallet;

public sealed record WalletDto(
    decimal Balance,
    string Currency,
    int Version,
    DateTimeOffset? UpdatedAt
);
