using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Notifications.Infrastructure.Email;

public sealed class StorefrontOptions
{
    public const string SectionName = "Storefront";

    [Required]
    public Uri BaseUrl { get; init; } = new("http://localhost:3000");
}
