using OrderFlow.Payments.Domain.EventSourcing;

namespace OrderFlow.Payments.Domain.Wallets;

public sealed record WalletOpened(string OwnerId, string Currency, DateTimeOffset OccurredAt)
    : IDomainEvent;

public sealed record FundsDeposited(
    Guid DepositId,
    decimal Amount,
    string Currency,
    string CardBrand,
    string CardLast4,
    string AuthorizationCode,
    DateTimeOffset OccurredAt
) : IDomainEvent;

public sealed record FundsWithdrawn(
    Guid OrderId,
    decimal Amount,
    string Currency,
    DateTimeOffset OccurredAt
) : IDomainEvent;

public sealed record FundsRefunded(
    Guid OrderId,
    Guid RefundId,
    decimal Amount,
    string Currency,
    DateTimeOffset OccurredAt
) : IDomainEvent;

public sealed record DepositSettled(Guid DepositId, DateTimeOffset OccurredAt) : IDomainEvent;

public sealed record DepositReversed(
    Guid DepositId,
    Guid ReversalId,
    decimal Amount,
    string Currency,
    string ReasonCode,
    DateTimeOffset OccurredAt
) : IDomainEvent;
