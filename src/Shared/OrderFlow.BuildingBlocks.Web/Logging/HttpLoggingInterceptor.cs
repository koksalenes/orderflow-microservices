using Microsoft.AspNetCore.HttpLogging;

namespace OrderFlow.BuildingBlocks.Web.Logging;

internal sealed class HttpLoggingInterceptor : IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
        var request = logContext.HttpContext.Request;

        if (request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase))
        {
            logContext.LoggingFields = HttpLoggingFields.None;
            return ValueTask.CompletedTask;
        }

        logContext.AddParameter("ClientIp", logContext.HttpContext.ClientIp());
        return ValueTask.CompletedTask;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext) =>
        ValueTask.CompletedTask;
}
