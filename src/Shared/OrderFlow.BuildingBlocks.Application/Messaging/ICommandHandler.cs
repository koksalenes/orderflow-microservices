using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.BuildingBlocks.Application.Messaging;

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : notnull
{
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
