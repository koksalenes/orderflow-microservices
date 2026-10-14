using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Gateway.Http;

internal sealed class RequestLimitsOptions
{
    public const string SectionName = "RequestLimits";

    [Range(1024, 104_857_600)]
    public long MaxRequestBodyBytes { get; init; } = 1_048_576;
}
