namespace OrderFlow.Payments.Application.Wallets.DepositFunds;

public sealed record DepositResultDto(
    Guid DepositId,
    decimal Amount,
    string Currency,
    decimal Balance,
    string CardBrand,
    string CardLast4,
    string AuthorizationCode,
    int WalletVersion
);
