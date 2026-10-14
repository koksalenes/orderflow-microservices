using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Application.Messaging;

namespace OrderFlow.BuildingBlocks.Infrastructure.Messaging;

public sealed class EfUnitOfWork<TContext>(TContext context, Outbox outbox) : IUnitOfWork
    where TContext : DbContext
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
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

public static class UnitOfWorkExtensions
{
    public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddOutbox();
        services.AddScoped<IUnitOfWork, EfUnitOfWork<TContext>>();

        return services;
    }
}
