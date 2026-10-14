using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Payments.Application.CardPayments;
using OrderFlow.Payments.Application.Cards;
using OrderFlow.Payments.Contracts.Orders;

namespace OrderFlow.Payments.Application.Orders;

internal sealed class VoidCardPaymentHandler(
    ICardPaymentRepository cardPayments,
    IPaymentGateway paymentGateway,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider
) : IIntegrationMessageHandler<VoidCardPayment>
{
    public async Task HandleAsync(
        VoidCardPayment message,
        MessageContext context,
        CancellationToken cancellationToken
    )
    {
        var payment = await cardPayments
            .GetAsync(message.CardPaymentId, cancellationToken)
            .ConfigureAwait(false);
        if (payment is null || !payment.Void(timeProvider.GetUtcNow()))
        {
            return;
        }

        await paymentGateway
            .VoidAsync(payment.AuthorizationCode, cancellationToken)
            .ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
