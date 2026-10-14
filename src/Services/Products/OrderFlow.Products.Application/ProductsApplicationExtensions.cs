using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace OrderFlow.Products.Application;

public static class ProductsApplicationExtensions
{
    public static IServiceCollection AddProductsApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(ProductsApplicationExtensions).Assembly,
            includeInternalTypes: true
        );

        return services;
    }
}
