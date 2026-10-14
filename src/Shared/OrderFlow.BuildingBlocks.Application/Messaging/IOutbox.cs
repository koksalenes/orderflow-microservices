namespace OrderFlow.BuildingBlocks.Application.Messaging;

public interface IOutbox
{
    void Enqueue<TMessage>(string messageType, TMessage message)
        where TMessage : class;
}
