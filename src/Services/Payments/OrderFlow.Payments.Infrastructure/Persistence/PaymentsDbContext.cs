using Microsoft.EntityFrameworkCore;
using OrderFlow.BuildingBlocks.Infrastructure.Messaging;
using OrderFlow.Payments.Domain.Deposits;

namespace OrderFlow.Payments.Infrastructure.Persistence;

internal sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
    : DbContext(options)
{
    public DbSet<DepositSaga> DepositSagas => Set<DepositSaga>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly)
            .AddOutboxMessages()
            .AddInboxMessages();
}
