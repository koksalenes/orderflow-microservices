using OrderFlow.Payments.Domain.Deposits;

namespace OrderFlow.Payments.Application.Deposits;

public interface IDepositSagaRepository
{
    Task<DepositSaga?> GetAsync(Guid depositId, CancellationToken cancellationToken);

    void Add(DepositSaga saga);

    Task SaveAsync(CancellationToken cancellationToken);
}
