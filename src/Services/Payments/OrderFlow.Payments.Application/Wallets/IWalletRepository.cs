using OrderFlow.BuildingBlocks.Domain;
using OrderFlow.Payments.Domain.Wallets;

namespace OrderFlow.Payments.Application.Wallets;

public interface IWalletRepository
{
    Task<Wallet?> GetAsync(string ownerId, CancellationToken cancellationToken);

    Task<Result> SaveAsync(Wallet wallet, CancellationToken cancellationToken);
}
