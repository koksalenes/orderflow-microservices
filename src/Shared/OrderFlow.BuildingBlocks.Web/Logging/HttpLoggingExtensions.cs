using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderFlow.BuildingBlocks.ServiceDefaults.Correlation;

namespace OrderFlow.BuildingBlocks.Web.Logging;

internal static class HttpLoggingExtensions
{
    private const HttpLoggingFields DefaultFields =
        HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.RequestProtocol
        | HttpLoggingFields.RequestScheme
        | HttpLoggingFields.RequestHeaders
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.ResponseHeaders
        | HttpLoggingFields.Duration;

    public static IServiceCollection AddHttpLoggingDefaults(this IServiceCollection services)
    {
        services
            .AddOptions<HttpLoggingSettings>()
            .BindConfiguration(HttpLoggingSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpLogging(_ => { });
        services.AddHttpLoggingInterceptor<HttpLoggingInterceptor>();

        services
            .AddOptions<HttpLoggingOptions>()
            .Configure<IOptions<HttpLoggingSettings>>(
                (options, settings) =>
                {
                    options.CombineLogs = true;
                    options.LoggingFields = settings.Value.LogBodies
                        ? DefaultFields
                            | HttpLoggingFields.RequestBody
                            | HttpLoggingFields.ResponseBody
                        : DefaultFields;

                    options.RequestHeaders.Add(CorrelationId.HeaderName);
                    options.RequestHeaders.Add("X-Forwarded-For");
                    options.RequestHeaders.Add("X-Forwarded-Proto");
                    options.ResponseHeaders.Add(CorrelationId.HeaderName);
                    options.ResponseHeaders.Add("Location");
                    options.ResponseHeaders.Add("Retry-After");

                    options.MediaTypeOptions.AddText("application/json");
                    options.MediaTypeOptions.AddText("application/problem+json");
                    options.RequestBodyLogLimit = settings.Value.BodyLogLimit;
                    options.ResponseBodyLogLimit = settings.Value.BodyLogLimit;
                }
            );

        return services;
    }
}
