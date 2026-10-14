using OrderFlow.Payments.Application.Cards;

namespace OrderFlow.Payments.Application.Wallets.DepositFunds;

public sealed record DepositFundsCommand(
    string OwnerId,
    Guid DepositId,
    decimal Amount,
    CardDetails Card,
    ReceiptRecipient? Recipient = null
)
{
    public const decimal MinAmount = 1m;
    public const decimal MaxAmount = 10_000m;
}
