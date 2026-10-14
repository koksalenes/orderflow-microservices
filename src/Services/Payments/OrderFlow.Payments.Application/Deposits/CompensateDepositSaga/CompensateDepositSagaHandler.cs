using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Payments.Application.Cards;
using OrderFlow.Payments.Application.Wallets;
using OrderFlow.Payments.Contracts.Settlement;
using OrderFlow.Payments.Contracts.Wallets;
using OrderFlow.Payments.Domain.Deposits;

namespace OrderFlow.Payments.Application.Deposits.CompensateDepositSaga;

internal sealed class CompensateDepositSagaHandler(
    IDepositSagaRepository sagas,
    IWalletRepository wallets,
    IPaymentGateway paymentGateway,
    IOutbox outbox,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<CardPaymentSettlementFailed>
{
    public async Task HandleAsync(
        CardPaymentSettlementFailed message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        var saga = await sagas.GetAsync(message.DepositId, cancellationToken).ConfigureAwait(false);
        if (saga is null || saga.IsFinished)
        {
            return;
        }

        if (saga.State == DepositSagaState.AwaitingSettlement)
        {
            saga.BeginCompensation(message.ReasonCode, timeProvider.GetUtcNow());
            await sagas.SaveAsync(cancellationToken).ConfigureAwait(false);
        }

        await ReverseWalletAsync(saga, cancellationToken).ConfigureAwait(false);

        var voided = await paymentGateway
            .VoidAsync(saga.AuthorizationCode, cancellationToken)
            .ConfigureAwait(false);
        if (voided.IsFailure)
        {
            throw new SagaStepFailedException(
                saga.Id,
                DepositSaga.StepAuthorizationVoided,
                voided.Error!
            );
        }

        saga.RecordCompensationStep(
            DepositSaga.StepAuthorizationVoided,
            timeProvider.GetUtcNow(),
            saga.AuthorizationCode
        );
        saga.MarkCompensated(timeProvider.GetUtcNow());
        await sagas.SaveAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task ReverseWalletAsync(DepositSaga saga, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var wallet =
            await wallets.GetAsync(saga.OwnerId, cancellationToken).ConfigureAwait(false)
            ?? throw new SagaStepFailedException(
                $"Wallet of deposit saga '{saga.Id}' was not found."
            );

        var reversal = wallet.ReverseDeposit(
            saga.Id,
            Guid.CreateVersion7(now),
            saga.FailureReason ?? "settlement_failed",
            now
        );
        if (reversal.IsSuccess)
        {
            outbox.Enqueue(
                WalletDepositReversed.MessageType,
                new WalletDepositReversed(
                    saga.Id,
                    saga.OwnerId,
                    saga.RecipientEmail,
                    saga.RecipientName,
                    saga.Locale,
                    saga.Amount,
                    saga.Currency,
                    wallet.Balance,
                    saga.CardBrand,
                    saga.CardLast4,
                    saga.FailureReason ?? "settlement_failed",
                    now
                )
            );

            var saved = await wallets.SaveAsync(wallet, cancellationToken).ConfigureAwait(false);
            if (saved.IsFailure)
            {
                throw new SagaStepFailedException(
                    saga.Id,
                    DepositSaga.StepWalletReversed,
                    saved.Error!
                );
            }
        }

        saga.RecordCompensationStep(
            DepositSaga.StepWalletReversed,
            now,
            $"-{saga.Amount} {saga.Currency}"
        );
        await sagas.SaveAsync(cancellationToken).ConfigureAwait(false);
    }
}
