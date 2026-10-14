namespace OrderFlow.Payments.Contracts.Orders;

public static class PaymentMethods
{
    public const string Wallet = "wallet";
    public const string Card = "card";
}

public sealed record ChargeOrder(
    Guid OrderId,
    string CustomerId,
    decimal Amount,
    string Currency,
    string Method,
    Guid? CardPaymentId
)
{
    public const string MessageType = "payments.charge_order.v1";
}

public sealed record OrderPaymentCaptured(Guid OrderId, string Method, string Reference)
{
    public const string MessageType = "payments.order_payment_captured.v1";
}

public sealed record OrderPaymentFailed(Guid OrderId, string ReasonCode)
{
    public const string MessageType = "payments.order_payment_failed.v1";
}

public sealed record RefundOrderPayment(
    Guid OrderId,
    Guid RefundId,
    string CustomerId,
    string Method,
    Guid? CardPaymentId
)
{
    public const string MessageType = "payments.refund_order_payment.v1";
}

public sealed record OrderPaymentRefunded(Guid OrderId, string Method)
{
    public const string MessageType = "payments.order_payment_refunded.v1";
}

public sealed record VoidCardPayment(Guid CardPaymentId, Guid OrderId)
{
    public const string MessageType = "payments.void_card_payment.v1";
}
