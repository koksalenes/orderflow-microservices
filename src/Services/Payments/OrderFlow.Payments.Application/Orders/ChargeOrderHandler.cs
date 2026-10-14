using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Payments.Application.CardPayments;
using OrderFlow.Payments.Application.Cards;
using OrderFlow.Payments.Application.Deposits;
using OrderFlow.Payments.Application.Wallets;
using OrderFlow.Payments.Contracts.Orders;
using OrderFlow.Payments.Domain.CardPayments;
using OrderFlow.Payments.Domain.Wallets;

namespace OrderFlow.Payments.Application.Orders;

internal sealed class ChargeOrderHandler(
    IWalletRepository wallets,
    ICardPaymentRepository cardPayments,
    IPaymentGateway paymentGateway,
    IUnitOfWork unitOfWork,
    IOutbox outbox,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<ChargeOrder>
{
    public const string CardCaptureDeclined = "card_capture_declined";

    public Task HandleAsync(
        ChargeOrder message,
        MessageContext context,
        CancellationToken cancellationToken
    ) =>
        message.Method == PaymentMethods.Card
            ? ChargeCardAsync(message, cancellationToken)
            : ChargeWalletAsync(message, cancellationToken);

    private async Task ChargeWalletAsync(ChargeOrder message, CancellationToken cancellationToken)
    {
        var wallet = await wallets
            .GetAsync(message.CustomerId, cancellationToken)
            .ConfigureAwait(false);
        if (wallet is null)
        {
            EnqueueFailed(message, "payments.insufficient_funds");
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var withdrawal = wallet.Withdraw(message.OrderId, message.Amount, timeProvider.GetUtcNow());
        if (withdrawal.IsFailure)
        {
            if (withdrawal.Error!.Code == WalletErrors.OrderAlreadyPaid(message.OrderId).Code)
            {
                EnqueueCaptured(message, wallet.StreamId);
            }
            else
            {
                EnqueueFailed(message, withdrawal.Error.Code);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        EnqueueCaptured(message, wallet.StreamId);
        var saved = await wallets.SaveAsync(wallet, cancellationToken).ConfigureAwait(false);
        if (saved.IsFailure)
        {
            throw new SagaStepFailedException(message.OrderId, "charge_wallet", saved.Error!);
        }
    }

    private async Task ChargeCardAsync(ChargeOrder message, CancellationToken cancellationToken)
    {
        var payment = message.CardPaymentId is { } id
            ? await cardPayments.GetAsync(id, cancellationToken).ConfigureAwait(false)
            : null;
        if (payment is null || payment.OwnerId != message.CustomerId)
        {
            EnqueueFailed(
                message,
                CardPaymentErrors.NotFound(message.CardPaymentId ?? Guid.Empty).Code
            );
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        if (payment.Status == CardPaymentStatus.Captured && payment.OrderId == message.OrderId)
        {
            EnqueueCaptured(message, payment.AuthorizationCode);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var now = timeProvider.GetUtcNow();
        if (payment.Status != CardPaymentStatus.Authorized || payment.Amount != message.Amount)
        {
            var reason = CardPaymentErrors
                .AmountMismatch(payment.Id, payment.Amount, message.Amount)
                .Code;
            if (payment.Status != CardPaymentStatus.Authorized)
            {
                reason = CardPaymentErrors.NotAuthorized(payment.Id, payment.Status).Code;
            }

            await VoidAsync(payment, cancellationToken).ConfigureAwait(false);
            EnqueueFailed(message, reason);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var capture = await paymentGateway
            .CaptureAsync(
                payment.AuthorizationCode,
                payment.Amount,
                payment.CardLast4,
                cancellationToken
            )
            .ConfigureAwait(false);
        if (capture.IsSuccess)
        {
            payment.Capture(message.OrderId, now);
            EnqueueCaptured(message, payment.AuthorizationCode);
        }
        else
        {
            await paymentGateway
                .VoidAsync(payment.AuthorizationCode, cancellationToken)
                .ConfigureAwait(false);
            payment.Decline(message.OrderId, CardCaptureDeclined, now);
            EnqueueFailed(message, CardCaptureDeclined);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task VoidAsync(CardPayment payment, CancellationToken cancellationToken)
    {
        if (payment.Void(timeProvider.GetUtcNow()))
        {
            await paymentGateway
                .VoidAsync(payment.AuthorizationCode, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private void EnqueueCaptured(ChargeOrder message, string reference) =>
        outbox.Enqueue(
            OrderPaymentCaptured.MessageType,
            new OrderPaymentCaptured(message.OrderId, message.Method, reference)
        );

    private void EnqueueFailed(ChargeOrder message, string reason) =>
        outbox.Enqueue(
            OrderPaymentFailed.MessageType,
            new OrderPaymentFailed(message.OrderId, reason)
        );
}
