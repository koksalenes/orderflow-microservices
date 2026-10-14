using System.Diagnostics;

namespace OrderFlow.BuildingBlocks.ServiceDefaults.Telemetry;

public static class MessagingTelemetry
{
    public const string ActivitySourceName = "OrderFlow.Messaging";

    public static readonly ActivitySource Source = new(ActivitySourceName);
}
