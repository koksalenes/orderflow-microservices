using System.ComponentModel.DataAnnotations;

namespace OrderFlow.BuildingBlocks.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    [Required]
    public string Host { get; init; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; init; } = 5672;

    [Required]
    public string VirtualHost { get; init; } = "orderflow";

    [Required]
    public string Username { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;

    [Required]
    public string Exchange { get; init; } = "orderflow.events";

    [Required]
    public string DeadLetterExchange { get; init; } = "orderflow.dead-letter";
}
