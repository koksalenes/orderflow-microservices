using Microsoft.Extensions.Logging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Payments.Application.Cards;

namespace OrderFlow.Payments.Infrastructure.Cards;

internal sealed partial class TestPaymentGateway(ILogger<TestPaymentGateway> logger)
    : IPaymentGateway
{
    public Task<Result<CardAuthorization>> ChargeAsync(
        CardCharge charge,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(charge);

        var authorizationCode =
            $"TEST-{charge.IdempotencyKey.ToString("N")[..8].ToUpperInvariant()}";
        LogApproved(
            charge.Amount,
            charge.Currency,
            charge.Card.Brand,
            charge.Card.Last4,
            authorizationCode
        );

        return Task.FromResult(Result.Success(new CardAuthorization(authorizationCode)));
    }

    public Task<Result> VoidAsync(string authorizationCode, CancellationToken cancellationToken)
    {
        LogVoided(authorizationCode);
        return Task.FromResult(Result.Success());
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Test authorization {AuthorizationCode} voided"
    )]
    private partial void LogVoided(string authorizationCode);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Test payment approved: {Amount} {Currency} on {CardBrand} ****{CardLast4} ({AuthorizationCode})"
    )]
    private partial void LogApproved(
        decimal amount,
        string currency,
        string cardBrand,
        string cardLast4,
        string authorizationCode
    );
}
