using FluentValidation;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.BuildingBlocks.Application.Validation;

internal sealed class ValidationCommandHandlerDecorator<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner,
    IEnumerable<IValidator<TCommand>> validators
) : ICommandHandler<TCommand, TResult>
    where TCommand : notnull
{
    public async Task<Result<TResult>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken
    )
    {
        var error = await validators
            .ValidateAsync(command, cancellationToken)
            .ConfigureAwait(false);
        return error is null
            ? await inner.HandleAsync(command, cancellationToken).ConfigureAwait(false)
            : Result.Failure<TResult>(error);
    }
}
