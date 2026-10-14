using System.ComponentModel.DataAnnotations;

namespace OrderFlow.BuildingBlocks.Web.Authentication;

public sealed class JwtAuthenticationOptions
{
    public const string SectionName = "Authentication";

    [Required]
    public string Authority { get; init; } = string.Empty;

    public string? MetadataAddress { get; init; }

    [Required]
    public string Audience { get; init; } = "orderflow-api";

    public bool RequireHttpsMetadata { get; init; } = true;
}
