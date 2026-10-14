namespace OrderFlow.Payments.Domain.EventSourcing;

public abstract class EventSourcedAggregate
{
    private readonly List<IDomainEvent> _pendingEvents = [];

    public string StreamId { get; protected set; } = string.Empty;

    public int Version { get; private set; }

    public IReadOnlyList<IDomainEvent> PendingEvents => _pendingEvents;

    public void MarkPendingEventsAsStored()
    {
        Version += _pendingEvents.Count;
        _pendingEvents.Clear();
    }

    protected void Apply(IDomainEvent domainEvent)
    {
        When(domainEvent);
        _pendingEvents.Add(domainEvent);
    }

    protected void Replay(IEnumerable<IDomainEvent> history)
    {
        ArgumentNullException.ThrowIfNull(history);

        foreach (var domainEvent in history)
        {
            When(domainEvent);
            Version++;
        }
    }

    protected void RestoreVersion(int version) => Version = version;

    protected abstract void When(IDomainEvent domainEvent);
}
