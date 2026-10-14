using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Infrastructure.Persistence;

namespace OrderFlow.Notifications.Infrastructure;

public static class NotificationsInfrastructureExtensions
{
    public static IServiceCollection AddNotificationsInfrastructure(
        this IServiceCollection services
    )
    {
        services.AddPostgres();

        return services;
    }
}
