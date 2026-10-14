using System.Diagnostics;
using Serilog.Context;

namespace OrderFlow.BuildingBlocks.ServiceDefaults.Correlation;

public static class CorrelationContext
{
    private static readonly AsyncLocal<string?> Value = new();

    public static string? Current => Value.Value;

    public static IDisposable Begin(string correlationId)
    {
        var previous = Value.Value;
        Value.Value = correlationId;
        Activity.Current?.SetTag(CorrelationId.TagName, correlationId);
        var logProperty = LogContext.PushProperty(CorrelationId.LogPropertyName, correlationId);

        return new Scope(previous, logProperty);
    }

    private sealed class Scope(string? previous, IDisposable logProperty) : IDisposable
    {
        public void Dispose()
        {
            logProperty.Dispose();
            Value.Value = previous;
        }
    }
}
