using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.BuildingBlocks.Application.Validation;
using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.BuildingBlocks.Web.Endpoints;

public static class ResultHttpExtensions
{
    public static Results<Ok<T>, ProblemHttpResult> ToHttpResult<T>(this Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error!.ToProblem();
    }

    public static ProblemHttpResult ToProblem(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        var problem = error is ValidationError validation
            ? new HttpValidationProblemDetails(
                validation.Errors.ToDictionary(
                    pair => CamelCasePath(pair.Key),
                    pair => pair.Value,
                    StringComparer.Ordinal
                )
            )
            : new ProblemDetails();

        problem.Status = StatusCode(error.Type);
        problem.Detail = error.Message;
        problem.Extensions["code"] = error.Code;

        if (error.Params is { Count: > 0 })
        {
            problem.Extensions["params"] = error.Params;
        }

        return TypedResults.Problem(problem);
    }

    private static string CamelCasePath(string propertyPath) =>
        string.Join('.', propertyPath.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));

    private static int StatusCode(ErrorType type) =>
        type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError,
        };
}
