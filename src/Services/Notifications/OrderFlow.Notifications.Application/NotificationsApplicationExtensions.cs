using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Notifications.Application.Deposits;
using OrderFlow.Notifications.Application.Orders;
using OrderFlow.Orders.Contracts.Orders;
using OrderFlow.Payments.Contracts.Wallets;

namespace OrderFlow.Notifications.Application;

public static class NotificationsApplicationExtensions
{
    public static IServiceCollection AddNotificationsApplication(this IServiceCollection services)
    {
        services.AddScoped<
            IIntegrationMessageHandler<WalletFundsDeposited>,
            WalletFundsDepositedHandler
        >();
        services.AddScoped<
            IIntegrationMessageHandler<WalletDepositReversed>,
            WalletDepositReversedHandler
        >();

        services.AddScoped<
            IIntegrationMessageHandler<OrderStatusChanged>,
            OrderStatusChangedHandler
        >();

        return services;
    }
}
