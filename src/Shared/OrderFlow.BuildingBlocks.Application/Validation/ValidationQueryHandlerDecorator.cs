using FluentValidation;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.BuildingBlocks.Application.Validation;

internal sealed class ValidationQueryHandlerDecorator<TQuery, TResult>(
    IQueryHandler<TQuery, TResult> inner,
    IEnumerable<IValidator<TQuery>> validators
) : IQueryHandler<TQuery, TResult>
    where TQuery : notnull
{
    public async Task<Result<TResult>> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken
    )
    {
        var error = await validators.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        return error is null
            ? await inner.HandleAsync(query, cancellationToken).ConfigureAwait(false)
            : Result.Failure<TResult>(error);
    }
}
