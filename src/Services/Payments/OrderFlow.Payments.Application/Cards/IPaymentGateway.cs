using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Payments.Application.Cards;

public interface IPaymentGateway
{
    Task<Result<CardAuthorization>> ChargeAsync(
        CardCharge charge,
        CancellationToken cancellationToken
    );

    Task<Result> CaptureAsync(
        string authorizationCode,
        decimal amount,
        string cardLast4,
        CancellationToken cancellationToken
    );

    Task<Result> VoidAsync(string authorizationCode, CancellationToken cancellationToken);

    Task<Result> RefundAsync(
        string authorizationCode,
        decimal amount,
        CancellationToken cancellationToken
    );
}

public sealed record CardCharge(
    Guid IdempotencyKey,
    decimal Amount,
    string Currency,
    CardDetails Card
);

public sealed record CardAuthorization(string AuthorizationCode);
