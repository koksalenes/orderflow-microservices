using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Payments.Application.Cards;

namespace OrderFlow.Payments.Infrastructure.Cards;

internal sealed partial class TestPaymentGateway(
    IOptions<CardNetworkOptions> options,
    ILogger<TestPaymentGateway> logger
) : IPaymentGateway
{
    public Task<Result> CaptureAsync(
        string authorizationCode,
        decimal amount,
        string cardLast4,
        CancellationToken cancellationToken
    )
    {
        if (cardLast4 == options.Value.FailingCardLast4)
        {
            LogCaptureDeclined(authorizationCode);
            return Task.FromResult(
                Result.Failure(
                    Error.Failure(
                        "payments.card_capture_declined",
                        "The card issuer declined the capture."
                    )
                )
            );
        }

        LogCaptured(authorizationCode, amount);
        return Task.FromResult(Result.Success());
    }

    public Task<Result> RefundAsync(
        string authorizationCode,
        decimal amount,
        CancellationToken cancellationToken
    )
    {
        LogRefunded(authorizationCode, amount);
        return Task.FromResult(Result.Success());
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Test authorization {AuthorizationCode} captured: {Amount}"
    )]
    private partial void LogCaptured(string authorizationCode, decimal amount);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Test capture of {AuthorizationCode} declined by the issuer"
    )]
    private partial void LogCaptureDeclined(string authorizationCode);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Test authorization {AuthorizationCode} refunded: {Amount}"
    )]
    private partial void LogRefunded(string authorizationCode, decimal amount);

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
