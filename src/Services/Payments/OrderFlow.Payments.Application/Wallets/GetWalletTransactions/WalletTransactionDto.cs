namespace OrderFlow.Payments.Application.Wallets.GetWalletTransactions;

public sealed record WalletTransactionDto(
    Guid Id,
    string Type,
    decimal Amount,
    decimal BalanceAfter,
    string Currency,
    string? CardBrand,
    string? CardLast4,
    string? Status,
    Guid? ReferenceId,
    string? Reason,
    DateTimeOffset OccurredAt
);
