using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrderFlow.BuildingBlocks.Infrastructure.Persistence;

public static class PostgresExtensions
{
    public static IServiceCollection AddPostgres(this IServiceCollection services)
    {
        services
            .AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddHealthChecks()
            .AddNpgSql(
                provider =>
                    provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString,
                name: "postgres",
                timeout: TimeSpan.FromSeconds(3)
            );

        return services;
    }
}
