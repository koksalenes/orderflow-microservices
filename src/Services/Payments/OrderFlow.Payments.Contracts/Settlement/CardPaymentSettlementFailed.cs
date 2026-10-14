namespace OrderFlow.Payments.Contracts.Settlement;

public sealed record CardPaymentSettlementFailed(Guid DepositId, string ReasonCode, string Message)
{
    public const string MessageType = "payments.card_payment_settlement_failed.v1";
}
