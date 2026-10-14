using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Bff.Authentication;

internal sealed class OidcSettings
{
    public const string SectionName = "Oidc";

    [Required]
    public string Authority { get; init; } = string.Empty;

    public string? MetadataAddress { get; init; }

    [Required]
    public string ClientId { get; init; } = string.Empty;

    [Required]
    public string ClientSecret { get; init; } = string.Empty;

    public bool RequireHttpsMetadata { get; init; } = true;
}
