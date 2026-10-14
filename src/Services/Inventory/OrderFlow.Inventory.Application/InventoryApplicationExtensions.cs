using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Inventory.Application.Reservations;
using OrderFlow.Inventory.Contracts.Reservations;

namespace OrderFlow.Inventory.Application;

public static class InventoryApplicationExtensions
{
    public static IServiceCollection AddInventoryApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(InventoryApplicationExtensions).Assembly,
            includeInternalTypes: true
        );

        services.AddScoped<IIntegrationMessageHandler<ReserveStock>, ReserveStockHandler>();
        services.AddScoped<IIntegrationMessageHandler<ReleaseStock>, ReleaseStockHandler>();
        services.AddScoped<IIntegrationMessageHandler<CommitStock>, CommitStockHandler>();

        return services;
    }
}
