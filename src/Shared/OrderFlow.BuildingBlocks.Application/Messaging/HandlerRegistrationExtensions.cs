using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Application.Validation;

namespace OrderFlow.BuildingBlocks.Application.Messaging;

public static class HandlerRegistrationExtensions
{
    public static IServiceCollection AddQueryHandler<TQuery, TResult, THandler>(
        this IServiceCollection services
    )
        where TQuery : notnull
        where THandler : class, IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<IQueryHandler<TQuery, TResult>>(provider =>
            ActivatorUtilities.CreateInstance<ValidationQueryHandlerDecorator<TQuery, TResult>>(
                provider,
                provider.GetRequiredService<THandler>()
            )
        );

        return services;
    }
}
