using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Payments.Application.Deposits.CompensateDepositSaga;
using OrderFlow.Payments.Application.Deposits.CompleteDepositSaga;
using OrderFlow.Payments.Application.Deposits.StartDepositSaga;
using OrderFlow.Payments.Application.Wallets.DepositFunds;
using OrderFlow.Payments.Contracts.Settlement;
using OrderFlow.Payments.Contracts.Wallets;

namespace OrderFlow.Payments.Application;

public static class PaymentsApplicationExtensions
{
    public static IServiceCollection AddPaymentsApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(PaymentsApplicationExtensions).Assembly,
            includeInternalTypes: true
        );
        services.AddCommandHandler<
            DepositFundsCommand,
            DepositResultDto,
            DepositFundsCommandHandler
        >();

        services.AddScoped<
            IIntegrationMessageHandler<WalletFundsDeposited>,
            StartDepositSagaHandler
        >();
        services.AddScoped<
            IIntegrationMessageHandler<CardPaymentSettled>,
            CompleteDepositSagaHandler
        >();
        services.AddScoped<
            IIntegrationMessageHandler<CardPaymentSettlementFailed>,
            CompensateDepositSagaHandler
        >();

        return services;
    }
}
