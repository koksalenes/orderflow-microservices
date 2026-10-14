using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

namespace OrderFlow.Gateway.Http;

internal static class RequestBodyLimitExtensions
{
    public static IServiceCollection AddRequestBodyLimit(this IServiceCollection services)
    {
        services
            .AddOptions<RequestLimitsOptions>()
            .BindConfiguration(RequestLimitsOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<KestrelServerOptions>()
            .Configure<IOptions<RequestLimitsOptions>>(
                (kestrel, limits) =>
                    kestrel.Limits.MaxRequestBodySize = limits.Value.MaxRequestBodyBytes
            );

        return services;
    }

    public static WebApplication UseRequestBodyLimit(this WebApplication app)
    {
        app.Use(
            (context, next) =>
            {
                var limit = context
                    .Features.Get<IHttpMaxRequestBodySizeFeature>()
                    ?.MaxRequestBodySize;

                if (limit is not null && context.Request.ContentLength > limit)
                {
                    context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                    return Task.CompletedTask;
                }

                return next(context);
            }
        );

        return app;
    }
}
