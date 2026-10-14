namespace OrderFlow.Payments.Domain.Wallets;

public sealed record WalletSnapshot(
    string OwnerId,
    string Currency,
    decimal Balance,
    IReadOnlyDictionary<Guid, decimal> PendingDeposits,
    IReadOnlyDictionary<Guid, decimal> OrderPayments
)
{
    public const int SchemaVersion = 3;
}
