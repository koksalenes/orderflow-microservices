using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.ServiceDefaults.Correlation;

namespace OrderFlow.BuildingBlocks.Web.Errors;

internal static class ProblemDetailsExtensions
{
    private const string ApiVersioningProblemTypePrefix =
        "https://docs.api-versioning.org/problems";

    public static IServiceCollection AddProblemDetailsDefaults(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = context =>
            {
                var problem = context.ProblemDetails;
                var http = context.HttpContext;
                var status = problem.Status ?? http.Response.StatusCode;

                problem.Instance ??= http.Request.Path;
                problem.Extensions["traceId"] =
                    Activity.Current?.TraceId.ToHexString() ?? http.TraceIdentifier;

                if (CorrelationContext.Current is { } correlationId)
                {
                    problem.Extensions["correlationId"] = correlationId;
                }

                if (
                    problem.Type?.StartsWith(
                        ApiVersioningProblemTypePrefix,
                        StringComparison.Ordinal
                    ) == true
                )
                {
                    problem.Extensions["code"] = ErrorCodes.UnsupportedApiVersion;
                }
                else
                {
                    problem.Extensions.TryAdd("code", ErrorCodes.FromStatusCode(status));
                }
            }
        );

        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }
}
