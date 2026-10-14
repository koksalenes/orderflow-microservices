using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Application.Paging;
using OrderFlow.BuildingBlocks.Infrastructure.Messaging;
using OrderFlow.BuildingBlocks.Infrastructure.Persistence;
using OrderFlow.Payments.Application;
using OrderFlow.Payments.Application.Cards;
using OrderFlow.Payments.Application.Deposits;
using OrderFlow.Payments.Application.Deposits.GetDepositSagas;
using OrderFlow.Payments.Application.Wallets;
using OrderFlow.Payments.Application.Wallets.GetWallet;
using OrderFlow.Payments.Application.Wallets.GetWalletEventStream;
using OrderFlow.Payments.Application.Wallets.GetWalletTransactions;
using OrderFlow.Payments.Contracts.Settlement;
using OrderFlow.Payments.Contracts.Wallets;
using OrderFlow.Payments.Infrastructure.Cards;
using OrderFlow.Payments.Infrastructure.Deposits;
using OrderFlow.Payments.Infrastructure.Deposits.GetDepositSagas;
using OrderFlow.Payments.Infrastructure.EventStore;
using OrderFlow.Payments.Infrastructure.Persistence;
using OrderFlow.Payments.Infrastructure.Wallets;
using OrderFlow.Payments.Infrastructure.Wallets.GetWallet;
using OrderFlow.Payments.Infrastructure.Wallets.GetWalletEventStream;
using OrderFlow.Payments.Infrastructure.Wallets.GetWalletTransactions;

namespace OrderFlow.Payments.Infrastructure;

public static class PaymentsInfrastructureExtensions
{
    public static IServiceCollection AddPaymentsInfrastructure(this IServiceCollection services)
    {
        services.AddPaymentsApplication();
        services.AddPostgresDbContext<PaymentsDbContext>();
        services
            .AddOptions<EventStoreOptions>()
            .BindConfiguration(EventStoreOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOutbox();
        services.AddScoped<IWalletRepository, EventSourcedWalletRepository>();
        services.AddScoped<IDepositSagaRepository, EfDepositSagaRepository>();
        services.AddSingleton<IPaymentGateway, TestPaymentGateway>();

        services.AddQueryHandler<GetWalletQuery, WalletDto, GetWalletQueryHandler>();
        services.AddQueryHandler<
            GetDepositSagasQuery,
            IReadOnlyList<DepositSagaDto>,
            GetDepositSagasQueryHandler
        >();
        services.AddQueryHandler<
            GetWalletTransactionsQuery,
            PagedResult<WalletTransactionDto>,
            GetWalletTransactionsQueryHandler
        >();
        services.AddQueryHandler<
            GetWalletEventStreamQuery,
            WalletEventStreamDto,
            GetWalletEventStreamQueryHandler
        >();

        return services;
    }

    public static IServiceCollection AddPaymentsMessaging(this IServiceCollection services)
    {
        services.AddOutboxPublisher();
        services
            .AddOptions<CardNetworkOptions>()
            .BindConfiguration(CardNetworkOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddScoped<IIntegrationMessageHandler<SettleCardPayment>, TestCardNetwork>();

        services.AddIntegrationConsumer<WalletFundsDeposited>(
            WalletFundsDeposited.MessageType,
            "payments.deposit_saga.wallet_funds_deposited"
        );
        services.AddIntegrationConsumer<SettleCardPayment>(
            SettleCardPayment.MessageType,
            "payments.card_network.settle_card_payment"
        );
        services.AddIntegrationConsumer<CardPaymentSettled>(
            CardPaymentSettled.MessageType,
            "payments.deposit_saga.card_payment_settled"
        );
        services.AddIntegrationConsumer<CardPaymentSettlementFailed>(
            CardPaymentSettlementFailed.MessageType,
            "payments.deposit_saga.card_payment_settlement_failed"
        );

        return services;
    }

    public static Task WaitForPaymentsDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default
    ) => services.WaitForMigrationsAsync<PaymentsDbContext>(cancellationToken);

    public static Task MigratePaymentsDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default
    ) => services.MigrateDatabaseAsync<PaymentsDbContext>(cancellationToken);
}
