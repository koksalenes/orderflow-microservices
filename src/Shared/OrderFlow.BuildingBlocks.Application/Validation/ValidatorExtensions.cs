using FluentValidation;

namespace OrderFlow.BuildingBlocks.Application.Validation;

internal static class ValidatorExtensions
{
    public static async Task<ValidationError?> ValidateAsync<T>(
        this IEnumerable<IValidator<T>> validators,
        T instance,
        CancellationToken cancellationToken
    )
    {
        var context = new ValidationContext<T>(instance);
        var failures = new List<FluentValidation.Results.ValidationFailure>();

        foreach (var validator in validators)
        {
            var result = await validator
                .ValidateAsync(context, cancellationToken)
                .ConfigureAwait(false);
            failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
        {
            return null;
        }

        var errors = failures
            .GroupBy(failure => failure.PropertyName, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorCode).Distinct().ToArray(),
                StringComparer.Ordinal
            );

        return new ValidationError(errors);
    }
}
