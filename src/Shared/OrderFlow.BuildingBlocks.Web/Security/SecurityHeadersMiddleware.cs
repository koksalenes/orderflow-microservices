using Microsoft.AspNetCore.Http;

namespace OrderFlow.BuildingBlocks.Web.Security;

/**
 * Adds the security headers every API response carries.
 * OnStarting keeps them on error responses too, after the exception handler clears the response.
 */
internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    private const string ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers.ContentSecurityPolicy = ContentSecurityPolicy;
            return Task.CompletedTask;
        });

        return next(context);
    }
}
