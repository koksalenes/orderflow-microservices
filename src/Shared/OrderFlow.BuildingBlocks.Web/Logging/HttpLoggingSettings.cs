using System.ComponentModel.DataAnnotations;

namespace OrderFlow.BuildingBlocks.Web.Logging;

public sealed class HttpLoggingSettings
{
    public const string SectionName = "HttpLogging";

    public bool LogBodies { get; init; }

    [Range(256, 65_536)]
    public int BodyLogLimit { get; init; } = 4096;
}
