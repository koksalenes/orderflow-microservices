using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.BuildingBlocks.Application.Messaging;

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : notnull
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
