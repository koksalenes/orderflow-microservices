using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderFlow.BuildingBlocks.Infrastructure.Messaging;
using OrderFlow.Payments.Application.Deposits;
using OrderFlow.Payments.Domain.Deposits;
using OrderFlow.Payments.Infrastructure.Persistence;

namespace OrderFlow.Payments.Infrastructure.Deposits;

internal sealed class EfDepositSagaRepository(PaymentsDbContext context, Outbox outbox)
    : IDepositSagaRepository
{
    public Task<DepositSaga?> GetAsync(Guid depositId, CancellationToken cancellationToken) =>
        context.DepositSagas.SingleOrDefaultAsync(saga => saga.Id == depositId, cancellationToken);

    public void Add(DepositSaga saga) => context.DepositSagas.Add(saga);

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        var transaction = await context
            .Database.BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using (transaction.ConfigureAwait(false))
        {
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await outbox
                .WriteAsync(
                    context.Database.GetDbConnection(),
                    transaction.GetDbTransaction(),
                    cancellationToken
                )
                .ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
