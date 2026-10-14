using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Application.Messaging;

namespace OrderFlow.BuildingBlocks.Infrastructure.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddOutbox(this IServiceCollection services)
    {
        services.AddScoped<Outbox>();
        services.AddScoped<IOutbox>(provider => provider.GetRequiredService<Outbox>());

        return services;
    }

    public static IServiceCollection AddOutboxPublisher(this IServiceCollection services)
    {
        services.AddRabbitMq();
        services
            .AddOptions<OutboxOptions>()
            .BindConfiguration(OutboxOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddHostedService<OutboxPublisher>();

        return services;
    }

    public static IServiceCollection AddIntegrationConsumer<TMessage>(
        this IServiceCollection services,
        string messageType,
        string queue
    )
        where TMessage : class
    {
        services.AddRabbitMq();
        services.AddHostedService(provider =>
            ActivatorUtilities.CreateInstance<RabbitMqConsumer<TMessage>>(
                provider,
                new ConsumerRegistration(messageType, queue)
            )
        );

        return services;
    }

    private static void AddRabbitMq(this IServiceCollection services)
    {
        if (services.Any(descriptor => descriptor.ServiceType == typeof(RabbitMqConnection)))
        {
            return;
        }

        services
            .AddOptions<RabbitMqOptions>()
            .BindConfiguration(RabbitMqOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<RabbitMqConnection>();
        services
            .AddHealthChecks()
            .AddCheck<RabbitMqHealthCheck>("rabbitmq", timeout: TimeSpan.FromSeconds(3));
    }
}
