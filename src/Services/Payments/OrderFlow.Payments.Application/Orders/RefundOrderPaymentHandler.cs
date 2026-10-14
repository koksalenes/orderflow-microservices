using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Payments.Application.CardPayments;
using OrderFlow.Payments.Application.Cards;
using OrderFlow.Payments.Application.Deposits;
using OrderFlow.Payments.Application.Wallets;
using OrderFlow.Payments.Contracts.Orders;

namespace OrderFlow.Payments.Application.Orders;

internal sealed class RefundOrderPaymentHandler(
    IWalletRepository wallets,
    ICardPaymentRepository cardPayments,
    IPaymentGateway paymentGateway,
    IUnitOfWork unitOfWork,
    IOutbox outbox,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<RefundOrderPayment>
{
    public async Task HandleAsync(
        RefundOrderPayment message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        var refunded = new OrderPaymentRefunded(message.OrderId, message.Method);

        if (message.Method == PaymentMethods.Card)
        {
            var payment = message.CardPaymentId is { } id
                ? await cardPayments.GetAsync(id, cancellationToken).ConfigureAwait(false)
                : null;
            if (payment is not null && payment.Refund(timeProvider.GetUtcNow()))
            {
                var refund = await paymentGateway
                    .RefundAsync(payment.AuthorizationCode, payment.Amount, cancellationToken)
                    .ConfigureAwait(false);
                if (refund.IsFailure)
                {
                    throw new SagaStepFailedException(
                        message.OrderId,
                        "refund_card",
                        refund.Error!
                    );
                }
            }

            outbox.Enqueue(OrderPaymentRefunded.MessageType, refunded);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var wallet = await wallets
            .GetAsync(message.CustomerId, cancellationToken)
            .ConfigureAwait(false);
        outbox.Enqueue(OrderPaymentRefunded.MessageType, refunded);
        if (
            wallet is null
            || wallet.Refund(message.OrderId, message.RefundId, timeProvider.GetUtcNow()).IsFailure
        )
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var saved = await wallets.SaveAsync(wallet, cancellationToken).ConfigureAwait(false);
        if (saved.IsFailure)
        {
            throw new SagaStepFailedException(message.OrderId, "refund_wallet", saved.Error!);
        }
    }
}
