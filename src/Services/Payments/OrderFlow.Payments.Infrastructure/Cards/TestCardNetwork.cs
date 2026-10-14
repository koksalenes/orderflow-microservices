using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Infrastructure.Messaging;
using OrderFlow.Payments.Contracts.Settlement;

namespace OrderFlow.Payments.Infrastructure.Cards;

internal sealed partial class TestCardNetwork(
    NpgsqlDataSource dataSource,
    Outbox outbox,
    IOptions<CardNetworkOptions> options,
    TimeProvider timeProvider,
    ILogger<TestCardNetwork> logger
) : IIntegrationMessageHandler<SettleCardPayment>
{
    public const string IssuerDeclined = "issuer_declined_settlement";

    public async Task HandleAsync(
        SettleCardPayment message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        var settings = options.Value;
        await Task.Delay(
                TimeSpan.FromSeconds(settings.SettlementDelaySeconds),
                timeProvider,
                cancellationToken
            )
            .ConfigureAwait(false);

        if (message.CardLast4 == settings.FailingCardLast4)
        {
            LogDeclined(message.DepositId, message.CardBrand, message.CardLast4);
            outbox.Enqueue(
                CardPaymentSettlementFailed.MessageType,
                new CardPaymentSettlementFailed(
                    message.DepositId,
                    IssuerDeclined,
                    "The card issuer declined the settlement of this authorization."
                )
            );
        }
        else
        {
            LogSettled(message.DepositId, message.CardBrand, message.CardLast4);
            outbox.Enqueue(
                CardPaymentSettled.MessageType,
                new CardPaymentSettled(message.DepositId, $"SETTLE-{message.AuthorizationCode}")
            );
        }

        var connection = await dataSource
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var transaction = await connection
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);
            await using (transaction.ConfigureAwait(false))
            {
                await outbox
                    .WriteAsync(connection, transaction, cancellationToken)
                    .ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Test card network settled deposit {DepositId} on {CardBrand} ****{CardLast4}"
    )]
    private partial void LogSettled(Guid depositId, string cardBrand, string cardLast4);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Test card network declined settlement of deposit {DepositId} on {CardBrand} ****{CardLast4}"
    )]
    private partial void LogDeclined(Guid depositId, string cardBrand, string cardLast4);
}
