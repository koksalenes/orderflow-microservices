using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace OrderFlow.Bff.Session;

internal static class SessionExtensions
{
    private const string DataProtectionKeysKey = "orderflow:bff:data-protection-keys";

    /**
     * Redis holds the server-side sessions and the Data Protection keys, so sessions survive
     * restarts and several BFF instances can share them.
     */
    public static IServiceCollection AddBffSession(this IServiceCollection services)
    {
        services
            .AddOptions<RedisOptions>()
            .BindConfiguration(RedisOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<BffSessionOptions>()
            .BindConfiguration(BffSessionOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var configuration = ConfigurationOptions.Parse(
                provider.GetRequiredService<IOptions<RedisOptions>>().Value.ConnectionString
            );
            configuration.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(configuration);
        });

        services.AddStackExchangeRedisCache(_ => { });
        services
            .AddOptions<RedisCacheOptions>()
            .Configure<IServiceProvider>(
                (options, provider) =>
                {
                    options.InstanceName = "orderflow:";
                    options.ConnectionMultiplexerFactory = () =>
                        Task.FromResult(provider.GetRequiredService<IConnectionMultiplexer>());
                }
            );

        services.AddDataProtection().SetApplicationName("OrderFlow.Bff");
        services
            .AddOptions<KeyManagementOptions>()
            .Configure<IServiceProvider>(
                (options, provider) =>
                    options.XmlRepository = new RedisXmlRepository(
                        () => provider.GetRequiredService<IConnectionMultiplexer>().GetDatabase(),
                        DataProtectionKeysKey
                    )
            );

        services.AddSingleton<RedisTicketStore>();

        services
            .AddHealthChecks()
            .AddRedis(
                provider => provider.GetRequiredService<IConnectionMultiplexer>(),
                name: "redis"
            );

        return services;
    }
}
