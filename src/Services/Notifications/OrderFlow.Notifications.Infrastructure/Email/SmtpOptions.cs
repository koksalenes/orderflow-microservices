using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Notifications.Infrastructure.Email;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    [Required]
    public string Host { get; init; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; init; } = 1025;

    public bool UseTls { get; init; }

    public string? Username { get; init; }

    public string? Password { get; init; }

    [Required, EmailAddress]
    public string FromAddress { get; init; } = "no-reply@orderflow.local";

    [Required]
    public string FromName { get; init; } = "OrderFlow";
}
