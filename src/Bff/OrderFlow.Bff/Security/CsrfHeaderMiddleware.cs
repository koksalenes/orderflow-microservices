using Microsoft.AspNetCore.Mvc;
using OrderFlow.BuildingBlocks.Web.Errors;

namespace OrderFlow.Bff.Security;

internal sealed class CsrfHeaderMiddleware(
    RequestDelegate next,
    IProblemDetailsService problemDetails
)
{
    public const string HeaderName = "X-CSRF";

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        var isProtected = path.StartsWithSegments("/api") || path.StartsWithSegments("/bff/user");

        if (
            isProtected
            && !string.Equals(context.Request.Headers[HeaderName], "1", StringComparison.Ordinal)
        )
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await problemDetails
                .WriteAsync(
                    new ProblemDetailsContext
                    {
                        HttpContext = context,
                        ProblemDetails = new ProblemDetails
                        {
                            Status = StatusCodes.Status403Forbidden,
                            Title = "The X-CSRF header is required.",
                            Extensions = { ["code"] = ErrorCodes.CsrfHeaderMissing },
                        },
                    }
                )
                .ConfigureAwait(false);
            return;
        }

        await next(context).ConfigureAwait(false);
    }
}
