using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Payments.Infrastructure.Cards;

public sealed class CardNetworkOptions
{
    public const string SectionName = "CardNetwork";

    [Range(0, 120)]
    public int SettlementDelaySeconds { get; init; } = 8;

    [Required]
    public string FailingCardLast4 { get; init; } = "0119";
}
