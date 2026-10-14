using System.ComponentModel.DataAnnotations;

namespace OrderFlow.BuildingBlocks.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;

    public bool MigrateOnStartup { get; init; }
}
