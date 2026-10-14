namespace OrderFlow.Payments.Domain.Wallets;

public sealed record WalletSnapshot(
    string OwnerId,
    string Currency,
    decimal Balance,
    IReadOnlyDictionary<Guid, decimal> PendingDeposits
)
{
    public const int SchemaVersion = 2;
}
