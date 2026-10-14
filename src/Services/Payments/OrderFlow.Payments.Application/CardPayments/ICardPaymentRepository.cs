using OrderFlow.Payments.Domain.CardPayments;

namespace OrderFlow.Payments.Application.CardPayments;

public interface ICardPaymentRepository
{
    Task<CardPayment?> GetAsync(Guid id, CancellationToken cancellationToken);

    void Add(CardPayment payment);
}
