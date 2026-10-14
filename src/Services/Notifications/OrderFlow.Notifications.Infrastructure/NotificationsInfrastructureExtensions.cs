using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderFlow.BuildingBlocks.Infrastructure.Messaging;
using OrderFlow.BuildingBlocks.Infrastructure.Persistence;
using OrderFlow.Notifications.Application;
using OrderFlow.Notifications.Application.Emails;
using OrderFlow.Notifications.Infrastructure.Email;
using OrderFlow.Notifications.Infrastructure.Persistence;
using OrderFlow.Payments.Contracts.Wallets;

namespace OrderFlow.Notifications.Infrastructure;

public static class NotificationsInfrastructureExtensions
{
    public const string WalletFundsDepositedQueue = "notifications.wallet_funds_deposited";
    public const string WalletDepositReversedQueue = "notifications.wallet_deposit_reversed";

    public static IServiceCollection AddNotificationsInfrastructure(
        this IServiceCollection services
    )
    {
        services.AddPostgresDbContext<NotificationsDbContext>();

        return services;
    }

    public static IServiceCollection AddNotificationsMessaging(this IServiceCollection services)
    {
        services.AddNotificationsApplication();

        services
            .AddOptions<SmtpOptions>()
            .BindConfiguration(SmtpOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services
            .AddOptions<StorefrontOptions>()
            .BindConfiguration(StorefrontOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton(provider => new StorefrontLinks(
            provider.GetRequiredService<IOptions<StorefrontOptions>>().Value.BaseUrl
        ));
        services.AddSingleton<IEmailSender, SmtpEmailSender>();

        services.AddIntegrationConsumer<WalletFundsDeposited>(
            WalletFundsDeposited.MessageType,
            WalletFundsDepositedQueue
        );
        services.AddIntegrationConsumer<WalletDepositReversed>(
            WalletDepositReversed.MessageType,
            WalletDepositReversedQueue
        );

        return services;
    }

    public static Task MigrateNotificationsDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default
    ) => services.MigrateDatabaseAsync<NotificationsDbContext>(cancellationToken);
}
