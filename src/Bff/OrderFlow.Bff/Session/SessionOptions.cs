using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Bff.Session;

internal sealed class SessionOptions
{
    public const string SectionName = "Session";

    [Required]
    public string CookieName { get; init; } = "orderflow.session";

    [Range(1, 720)]
    public int LifetimeHours { get; init; } = 336;
}
