namespace OrderFlow.BuildingBlocks.Application.Messaging;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
