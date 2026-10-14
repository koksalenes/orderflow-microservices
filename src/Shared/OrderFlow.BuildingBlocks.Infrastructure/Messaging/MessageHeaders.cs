namespace OrderFlow.BuildingBlocks.Infrastructure.Messaging;

internal static class MessageHeaders
{
    public const string CorrelationId = "correlation-id";
    public const string TraceParent = "traceparent";
    public const string OccurredAt = "occurred-at";
    public const string RetryCount = "x-retry-count";
}
