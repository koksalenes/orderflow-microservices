using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Payments.Contracts.Settlement;
using OrderFlow.Payments.Contracts.Wallets;
using OrderFlow.Payments.Domain.Deposits;

namespace OrderFlow.Payments.Application.Deposits.StartDepositSaga;

internal sealed class StartDepositSagaHandler(
    IDepositSagaRepository sagas,
    IOutbox outbox,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<WalletFundsDeposited>
{
    public async Task HandleAsync(
        WalletFundsDeposited message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        if (
            await sagas.GetAsync(message.DepositId, cancellationToken).ConfigureAwait(false)
            is not null
        )
        {
            return;
        }

        var saga = DepositSaga.Start(
            message.DepositId,
            message.OwnerId,
            message.Amount,
            message.Currency,
            message.CardBrand,
            message.CardLast4,
            message.AuthorizationCode,
            message.RecipientEmail,
            message.RecipientName,
            message.Locale,
            timeProvider.GetUtcNow()
        );
        sagas.Add(saga);
        outbox.Enqueue(
            SettleCardPayment.MessageType,
            new SettleCardPayment(
                saga.Id,
                saga.AuthorizationCode,
                saga.Amount,
                saga.Currency,
                saga.CardBrand,
                saga.CardLast4
            )
        );

        await sagas.SaveAsync(cancellationToken).ConfigureAwait(false);
    }
}
