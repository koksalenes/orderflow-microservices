namespace OrderFlow.BuildingBlocks.Application.Messaging;

public interface IIntegrationMessageHandler<in TMessage>
    where TMessage : class
{
    Task HandleAsync(TMessage message, MessageContext context, CancellationToken cancellationToken);
}

public sealed record MessageContext(
    Guid MessageId,
    string MessageType,
    DateTimeOffset OccurredAt,
    string? CorrelationId
);
