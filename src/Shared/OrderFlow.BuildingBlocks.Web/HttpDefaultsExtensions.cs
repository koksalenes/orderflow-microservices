using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Web.Correlation;
using OrderFlow.BuildingBlocks.Web.Errors;
using OrderFlow.BuildingBlocks.Web.Logging;
using OrderFlow.BuildingBlocks.Web.Proxy;
using OrderFlow.BuildingBlocks.Web.Security;

namespace OrderFlow.BuildingBlocks.Web;

public static class HttpDefaultsExtensions
{
    public static WebApplicationBuilder AddHttpDefaults(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.WebHost.ConfigureKestrel(kestrel => kestrel.AddServerHeader = false);
        builder.Services.AddTrustedProxy();
        builder
            .Services.AddOptions<SecurityHeadersOptions>()
            .BindConfiguration(SecurityHeadersOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        builder.Services.AddProblemDetailsDefaults();
        builder.Services.AddHttpLoggingDefaults();

        return builder;
    }

    public static WebApplication UseHttpDefaults(
        this WebApplication app,
        CorrelationIdPolicy correlationIdPolicy = CorrelationIdPolicy.ReuseIncoming
    )
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<CorrelationIdMiddleware>(correlationIdPolicy);
        app.UseForwardedHeaders();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseWhen(context => !IsGrpc(context.Request), branch => branch.UseHttpLogging());
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        return app;
    }

    private static bool IsGrpc(HttpRequest request) =>
        request.ContentType?.StartsWith("application/grpc", StringComparison.OrdinalIgnoreCase)
        == true;
}
