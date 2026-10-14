using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Payments.Application.Cards;

public interface IPaymentGateway
{
    Task<Result<CardAuthorization>> ChargeAsync(
        CardCharge charge,
        CancellationToken cancellationToken
    );

    Task<Result> VoidAsync(string authorizationCode, CancellationToken cancellationToken);
}

public sealed record CardCharge(
    Guid IdempotencyKey,
    decimal Amount,
    string Currency,
    CardDetails Card
);

public sealed record CardAuthorization(string AuthorizationCode);
