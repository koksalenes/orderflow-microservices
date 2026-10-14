using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace OrderFlow.BuildingBlocks.Web.Security;

/**
 * Adds the security headers every response carries.
 * OnStarting keeps them on error responses too, after the exception handler clears the response.
 */
internal sealed class SecurityHeadersMiddleware(
    RequestDelegate next,
    IOptions<SecurityHeadersOptions> options
)
{
    public Task InvokeAsync(HttpContext context)
    {
        var contentSecurityPolicy = options.Value.ContentSecurityPolicy;

        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers.ContentSecurityPolicy = contentSecurityPolicy;
            return Task.CompletedTask;
        });

        return next(context);
    }
}
