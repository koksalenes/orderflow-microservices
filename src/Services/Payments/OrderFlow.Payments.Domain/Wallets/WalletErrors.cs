using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Payments.Domain.Wallets;

public static class WalletErrors
{
    public static Error InvalidAmount(decimal amount) =>
        new(
            "payments.invalid_amount",
            $"The amount {amount} must be positive.",
            ErrorType.Validation,
            new Dictionary<string, object?>(StringComparer.Ordinal) { ["amount"] = amount }
        );

    public static Error BalanceLimitExceeded(decimal limit) =>
        new(
            "payments.balance_limit_exceeded",
            $"The balance cannot exceed {limit}.",
            ErrorType.Conflict,
            new Dictionary<string, object?>(StringComparer.Ordinal) { ["limit"] = limit }
        );

    public static Error ConcurrencyConflict(string streamId) =>
        Error.Conflict(
            "payments.concurrency_conflict",
            $"The wallet '{streamId}' was changed by another request. Try again."
        );

    public static Error DepositNotPending(Guid depositId) =>
        Error.Conflict(
            "payments.deposit_not_pending",
            $"The deposit '{depositId}' is already settled or reversed."
        );

    public static Error DuplicateDeposit(Guid depositId) =>
        Error.Conflict(
            "payments.duplicate_deposit",
            $"The deposit '{depositId}' has already been processed."
        );
}
