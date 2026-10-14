using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Bff.Session;

internal sealed class RedisOptions
{
    public const string SectionName = "Redis";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;
}
