namespace OrderFlow.Payments.Domain.EventSourcing;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
