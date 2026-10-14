namespace OrderFlow.Payments.Contracts.Settlement;

public sealed record CardPaymentSettled(Guid DepositId, string SettlementReference)
{
    public const string MessageType = "payments.card_payment_settled.v1";
}
