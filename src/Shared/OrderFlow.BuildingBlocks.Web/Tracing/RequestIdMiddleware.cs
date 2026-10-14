using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace OrderFlow.BuildingBlocks.Web.Tracing;

internal sealed class RequestIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Request-Id";

    public Task InvokeAsync(HttpContext context)
    {
        if (Activity.Current is { } activity)
        {
            context.TraceIdentifier = activity.TraceId.ToHexString();
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = context.TraceIdentifier;
            return Task.CompletedTask;
        });

        return next(context);
    }
}
