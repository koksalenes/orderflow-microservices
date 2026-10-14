using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OrderFlow.BuildingBlocks.Web.Errors;
using OrderFlow.BuildingBlocks.Web.Security;
using OrderFlow.BuildingBlocks.Web.Tracing;
using Serilog;
using Serilog.Events;

namespace OrderFlow.BuildingBlocks.Web;

public static class HttpDefaultsExtensions
{
    public static WebApplicationBuilder AddHttpDefaults(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.WebHost.ConfigureKestrel(kestrel => kestrel.AddServerHeader = false);
        builder.Services.AddProblemDetailsDefaults();

        return builder;
    }

    public static WebApplication UseHttpDefaults(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<RequestIdMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = GetRequestLogLevel;
            options.EnrichDiagnosticContext = (diagnostics, context) =>
            {
                diagnostics.Set("ClientIp", context.ClientIp());
                diagnostics.Set("Scheme", context.Request.Scheme);
                diagnostics.Set("UserAgent", context.Request.Headers.UserAgent.ToString());
            };
        });
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        return app;
    }

    private static LogEventLevel GetRequestLogLevel(
        HttpContext context,
        double elapsedMilliseconds,
        Exception? exception
    )
    {
        return exception is not null
            || context.Response.StatusCode >= StatusCodes.Status500InternalServerError
                ? LogEventLevel.Error
            : context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase)
                ? LogEventLevel.Verbose
            : LogEventLevel.Information;
    }
}
