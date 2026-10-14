using System.ComponentModel.DataAnnotations;

namespace OrderFlow.BuildingBlocks.Web.Security;

public sealed class SecurityHeadersOptions
{
    public const string SectionName = "SecurityHeaders";

    [Required]
    public string ContentSecurityPolicy { get; init; } =
        "default-src 'none'; frame-ancestors 'none'";
}
