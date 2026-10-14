using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Payments.Application.Cards;
using OrderFlow.Payments.Domain.CardPayments;
using OrderFlow.Payments.Domain.Wallets;

namespace OrderFlow.Payments.Application.CardPayments.AuthorizeCardPayment;

internal sealed class AuthorizeCardPaymentCommandHandler(
    IPaymentGateway paymentGateway,
    ICardPaymentRepository cardPayments,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider
) : ICommandHandler<AuthorizeCardPaymentCommand, CardPaymentDto>
{
    public async Task<Result<CardPaymentDto>> HandleAsync(
        AuthorizeCardPaymentCommand command,
        CancellationToken cancellationToken
    )
    {
        var now = timeProvider.GetUtcNow();
        var id = Guid.CreateVersion7(now);
        var authorization = await paymentGateway
            .ChargeAsync(
                new CardCharge(id, command.Amount, Wallet.DefaultCurrency, command.Card),
                cancellationToken
            )
            .ConfigureAwait(false);
        if (authorization.IsFailure)
        {
            return Result.Failure<CardPaymentDto>(authorization.Error!);
        }

        var payment = CardPayment.Authorize(
            id,
            command.OwnerId,
            command.Amount,
            Wallet.DefaultCurrency,
            command.Card.Brand,
            command.Card.Last4,
            authorization.Value.AuthorizationCode,
            now
        );
        cardPayments.Add(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(
            new CardPaymentDto(
                payment.Id,
                payment.Amount,
                payment.Currency,
                payment.CardBrand,
                payment.CardLast4,
                payment.AuthorizationCode,
                payment.Status.ToString()
            )
        );
    }
}
