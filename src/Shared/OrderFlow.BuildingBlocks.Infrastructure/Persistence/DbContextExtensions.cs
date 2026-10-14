using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace OrderFlow.BuildingBlocks.Infrastructure.Persistence;

public static class DbContextExtensions
{
    private const int MigrationWaitAttempts = 60;
    private static readonly TimeSpan MigrationWaitDelay = TimeSpan.FromSeconds(2);

    public static IServiceCollection AddPostgresDbContext<TContext>(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder>? configure = null
    )
        where TContext : DbContext
    {
        services.AddPostgres();
        services.AddSingleton(provider =>
            NpgsqlDataSource.Create(
                provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString
            )
        );
        services.AddDbContext<TContext>(
            (provider, options) =>
            {
                options
                    .UseNpgsql(provider.GetRequiredService<NpgsqlDataSource>())
                    .UseSnakeCaseNamingConvention();
                configure?.Invoke(provider, options);
            }
        );

        return services;
    }

    public static async Task MigrateDatabaseAsync<TContext>(
        this IServiceProvider services,
        CancellationToken cancellationToken = default
    )
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!services.GetRequiredService<IOptions<DatabaseOptions>>().Value.MigrateOnStartup)
        {
            return;
        }

        var scope = services.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            await scope
                .ServiceProvider.GetRequiredService<TContext>()
                .Database.MigrateAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public static async Task WaitForMigrationsAsync<TContext>(
        this IServiceProvider services,
        CancellationToken cancellationToken = default
    )
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!services.GetRequiredService<IOptions<DatabaseOptions>>().Value.MigrateOnStartup)
        {
            return;
        }

        var timeProvider = services.GetRequiredService<TimeProvider>();
        for (var attempt = 1; attempt <= MigrationWaitAttempts; attempt++)
        {
            if (await IsUpToDateAsync<TContext>(services, cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            await Task.Delay(MigrationWaitDelay, timeProvider, cancellationToken)
                .ConfigureAwait(false);
        }

        throw new InvalidOperationException(
            $"The {typeof(TContext).Name} migrations were not applied in time."
        );
    }

    private static async Task<bool> IsUpToDateAsync<TContext>(
        IServiceProvider services,
        CancellationToken cancellationToken
    )
        where TContext : DbContext
    {
        var scope = services.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            try
            {
                var pending = await scope
                    .ServiceProvider.GetRequiredService<TContext>()
                    .Database.GetPendingMigrationsAsync(cancellationToken)
                    .ConfigureAwait(false);
                return !pending.Any();
            }
            catch (NpgsqlException)
            {
                return false;
            }
        }
    }
}
