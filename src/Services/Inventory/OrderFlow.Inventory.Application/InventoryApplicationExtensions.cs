using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace OrderFlow.Inventory.Application;

public static class InventoryApplicationExtensions
{
    public static IServiceCollection AddInventoryApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(InventoryApplicationExtensions).Assembly,
            includeInternalTypes: true
        );

        return services;
    }
}
