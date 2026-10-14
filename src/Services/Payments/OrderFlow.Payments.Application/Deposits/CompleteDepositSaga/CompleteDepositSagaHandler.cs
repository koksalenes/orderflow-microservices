using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Payments.Application.Wallets;
using OrderFlow.Payments.Contracts.Settlement;
using OrderFlow.Payments.Domain.Deposits;

namespace OrderFlow.Payments.Application.Deposits.CompleteDepositSaga;

internal sealed class CompleteDepositSagaHandler(
    IDepositSagaRepository sagas,
    IWalletRepository wallets,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<CardPaymentSettled>
{
    public async Task HandleAsync(
        CardPaymentSettled message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        var saga = await sagas.GetAsync(message.DepositId, cancellationToken).ConfigureAwait(false);
        if (saga is null || saga.State != DepositSagaState.AwaitingSettlement)
        {
            return;
        }

        var now = timeProvider.GetUtcNow();
        var wallet =
            await wallets.GetAsync(saga.OwnerId, cancellationToken).ConfigureAwait(false)
            ?? throw new SagaStepFailedException(
                $"Wallet of deposit saga '{saga.Id}' was not found."
            );

        if (wallet.SettleDeposit(saga.Id, now).IsSuccess)
        {
            var saved = await wallets.SaveAsync(wallet, cancellationToken).ConfigureAwait(false);
            if (saved.IsFailure)
            {
                throw new SagaStepFailedException(saga.Id, DepositSaga.StepSettled, saved.Error!);
            }
        }

        saga.MarkSettled(now);
        await sagas.SaveAsync(cancellationToken).ConfigureAwait(false);
    }
}
