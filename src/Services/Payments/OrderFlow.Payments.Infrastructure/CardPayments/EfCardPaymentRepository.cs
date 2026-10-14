using Microsoft.EntityFrameworkCore;
using OrderFlow.Payments.Application.CardPayments;
using OrderFlow.Payments.Domain.CardPayments;
using OrderFlow.Payments.Infrastructure.Persistence;

namespace OrderFlow.Payments.Infrastructure.CardPayments;

internal sealed class EfCardPaymentRepository(PaymentsDbContext context) : ICardPaymentRepository
{
    public Task<CardPayment?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        context.CardPayments.SingleOrDefaultAsync(payment => payment.Id == id, cancellationToken);

    public void Add(CardPayment payment) => context.CardPayments.Add(payment);
}
