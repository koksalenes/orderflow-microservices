using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OrderFlow.BuildingBlocks.Web.Errors;

internal sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
    private const int ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (
            exception is OperationCanceledException
            && httpContext.RequestAborted.IsCancellationRequested
        )
        {
            LogRequestAborted(httpContext.Request.Method, httpContext.Request.Path);
            httpContext.Response.StatusCode = ClientClosedRequest;
            return true;
        }

        var problem = exception switch
        {
            BadHttpRequestException badRequest => new ProblemDetails
            {
                Status = badRequest.StatusCode,
                Title = "The request is invalid.",
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
            },
        };

        var status = problem.Status!.Value;
        problem.Extensions["code"] = ErrorCodes.FromStatusCode(status);

        if (status >= StatusCodes.Status500InternalServerError)
        {
            LogUnhandledException(exception, httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            LogBadRequest(exception.Message, httpContext.Request.Method, httpContext.Request.Path);
        }

        if (environment.IsDevelopment())
        {
            problem.Detail = exception.Message;
        }

        httpContext.Response.StatusCode = status;

        return await problemDetailsService
            .TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    Exception = exception,
                    ProblemDetails = problem,
                }
            )
            .ConfigureAwait(false);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception for {Method} {Path}")]
    private partial void LogUnhandledException(Exception exception, string method, PathString path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Bad request for {Method} {Path}: {Reason}")]
    private partial void LogBadRequest(string reason, string method, PathString path);

    [LoggerMessage(Level = LogLevel.Information, Message = "Client aborted {Method} {Path}")]
    private partial void LogRequestAborted(string method, PathString path);
}
