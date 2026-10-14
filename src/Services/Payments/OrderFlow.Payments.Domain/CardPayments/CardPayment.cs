using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Payments.Domain.CardPayments;

public sealed class CardPayment : AggregateRoot<Guid>
{
    private CardPayment() { }

    public string OwnerId { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public string CardBrand { get; private set; } = string.Empty;

    public string CardLast4 { get; private set; } = string.Empty;

    public string AuthorizationCode { get; private set; } = string.Empty;

    public CardPaymentStatus Status { get; private set; }

    public Guid? OrderId { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public uint Version { get; }

    public static CardPayment Authorize(
        Guid id,
        string ownerId,
        decimal amount,
        string currency,
        string cardBrand,
        string cardLast4,
        string authorizationCode,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = id,
            OwnerId = ownerId,
            Amount = amount,
            Currency = currency,
            CardBrand = cardBrand,
            CardLast4 = cardLast4,
            AuthorizationCode = authorizationCode,
            Status = CardPaymentStatus.Authorized,
            CreatedAt = now,
            UpdatedAt = now,
        };

    public Result Capture(Guid orderId, DateTimeOffset now)
    {
        if (Status != CardPaymentStatus.Authorized)
        {
            return Result.Failure(CardPaymentErrors.NotAuthorized(Id, Status));
        }

        Status = CardPaymentStatus.Captured;
        OrderId = orderId;
        UpdatedAt = now;
        return Result.Success();
    }

    public void Decline(Guid orderId, string reason, DateTimeOffset now)
    {
        Status = CardPaymentStatus.Declined;
        OrderId = orderId;
        FailureReason = reason;
        UpdatedAt = now;
    }

    public bool Void(DateTimeOffset now)
    {
        if (Status != CardPaymentStatus.Authorized)
        {
            return false;
        }

        Status = CardPaymentStatus.Voided;
        UpdatedAt = now;
        return true;
    }

    public bool Refund(DateTimeOffset now)
    {
        if (Status != CardPaymentStatus.Captured)
        {
            return false;
        }

        Status = CardPaymentStatus.Refunded;
        UpdatedAt = now;
        return true;
    }
}

public enum CardPaymentStatus
{
    Authorized,
    Captured,
    Declined,
    Voided,
    Refunded,
}
