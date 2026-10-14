namespace OrderFlow.BuildingBlocks.Domain;

public record Error(
    string Code,
    string Message,
    ErrorType Type,
    IReadOnlyDictionary<string, object?>? Params = null
)
{
    public static Error NotFound(
        string code,
        string message,
        IReadOnlyDictionary<string, object?>? parameters = null
    ) => new(code, message, ErrorType.NotFound, parameters);

    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);
}
