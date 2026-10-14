using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrderFlow.BuildingBlocks.ServiceDefaults.Correlation;
using OrderFlow.BuildingBlocks.Web.Proxy;

namespace OrderFlow.BuildingBlocks.Web.Correlation;

internal sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    TimeProvider timeProvider,
    IOptions<TrustedProxyOptions> trustedProxies,
    CorrelationIdPolicy policy
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var incoming = context.Request.Headers[CorrelationId.HeaderName].ToString();
        var canReuse =
            policy == CorrelationIdPolicy.ReuseIncoming
            || trustedProxies.Value.IsTrusted(context.Connection.RemoteIpAddress);

        var correlationId =
            canReuse && CorrelationId.IsValid(incoming)
                ? incoming
                : CorrelationId.New(timeProvider);

        context.Request.Headers[CorrelationId.HeaderName] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationId.HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (CorrelationContext.Begin(correlationId))
        {
            await next(context).ConfigureAwait(false);
        }
    }
}
