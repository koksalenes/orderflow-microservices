namespace OrderFlow.BuildingBlocks.Domain;

public class Result
{
    protected Result(Error? error) => Error = error;

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public bool IsFailure => !IsSuccess;

    public static Result Success() => new(null);

    public static Result<T> Success<T>(T value) => new(value, null);

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(error);
    }

    public static Result<T> Failure<T>(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(default, error);
    }
}

public sealed class Result<T> : Result
{
    internal Result(T? value, Error? error)
        : base(error) => Value = value!;

    public T Value
    {
        get =>
            IsSuccess
                ? field
                : throw new InvalidOperationException("A failed result has no value.");
        private init;
    }
}
