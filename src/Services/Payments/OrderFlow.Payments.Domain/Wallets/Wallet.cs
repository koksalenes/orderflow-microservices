using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Payments.Domain.EventSourcing;

namespace OrderFlow.Payments.Domain.Wallets;

public sealed class Wallet : EventSourcedAggregate
{
    public const string DefaultCurrency = "USD";
    public const decimal MaxBalance = 100_000m;

    private readonly Dictionary<Guid, decimal> _pendingDeposits = [];

    private Wallet() { }

    public string OwnerId { get; private set; } = string.Empty;

    public string Currency { get; private set; } = string.Empty;

    public decimal Balance { get; private set; }

    public IReadOnlyDictionary<Guid, decimal> PendingDeposits => _pendingDeposits;

    public static string StreamIdFor(string ownerId) => $"wallet-{ownerId}";

    public static Wallet Open(string ownerId, string currency, DateTimeOffset now)
    {
        var wallet = new Wallet();
        wallet.Apply(new WalletOpened(ownerId, currency, now));
        return wallet;
    }

    public static Wallet Rehydrate(
        WalletSnapshot? snapshot,
        int snapshotVersion,
        IEnumerable<IDomainEvent> eventsAfterSnapshot
    )
    {
        var wallet = new Wallet();
        if (snapshot is not null)
        {
            wallet.StreamId = StreamIdFor(snapshot.OwnerId);
            wallet.OwnerId = snapshot.OwnerId;
            wallet.Currency = snapshot.Currency;
            wallet.Balance = snapshot.Balance;
            foreach (var (depositId, amount) in snapshot.PendingDeposits)
            {
                wallet._pendingDeposits[depositId] = amount;
            }

            wallet.RestoreVersion(snapshotVersion);
        }

        wallet.Replay(eventsAfterSnapshot);
        return wallet;
    }

    public Result CanDeposit(decimal amount)
    {
        return amount <= 0
            ? Result.Failure(WalletErrors.InvalidAmount(amount))
            : Balance + amount > MaxBalance
            ? Result.Failure(WalletErrors.BalanceLimitExceeded(MaxBalance))
            : Result.Success();
    }

    public Result Deposit(
        Guid depositId,
        decimal amount,
        string cardBrand,
        string cardLast4,
        string authorizationCode,
        DateTimeOffset now
    )
    {
        var check = CanDeposit(amount);
        if (check.IsFailure)
        {
            return check;
        }

        Apply(
            new FundsDeposited(
                depositId,
                amount,
                Currency,
                cardBrand,
                cardLast4,
                authorizationCode,
                now
            )
        );
        return Result.Success();
    }

    public Result SettleDeposit(Guid depositId, DateTimeOffset now)
    {
        if (!_pendingDeposits.ContainsKey(depositId))
        {
            return Result.Failure(WalletErrors.DepositNotPending(depositId));
        }

        Apply(new DepositSettled(depositId, now));
        return Result.Success();
    }

    public Result ReverseDeposit(
        Guid depositId,
        Guid reversalId,
        string reasonCode,
        DateTimeOffset now
    )
    {
        if (!_pendingDeposits.TryGetValue(depositId, out var amount))
        {
            return Result.Failure(WalletErrors.DepositNotPending(depositId));
        }

        Apply(new DepositReversed(depositId, reversalId, amount, Currency, reasonCode, now));
        return Result.Success();
    }

    public WalletSnapshot ToSnapshot() =>
        new(OwnerId, Currency, Balance, new Dictionary<Guid, decimal>(_pendingDeposits));

    protected override void When(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case WalletOpened opened:
                StreamId = StreamIdFor(opened.OwnerId);
                OwnerId = opened.OwnerId;
                Currency = opened.Currency;
                Balance = 0;
                break;
            case FundsDeposited deposited:
                Balance += deposited.Amount;
                _pendingDeposits[deposited.DepositId] = deposited.Amount;
                break;
            case DepositSettled settled:
                _pendingDeposits.Remove(settled.DepositId);
                break;
            case DepositReversed reversed:
                Balance -= reversed.Amount;
                _pendingDeposits.Remove(reversed.DepositId);
                break;
            default:
                throw new InvalidOperationException(
                    $"Wallet cannot apply event '{domainEvent.GetType().Name}'."
                );
        }
    }
}
