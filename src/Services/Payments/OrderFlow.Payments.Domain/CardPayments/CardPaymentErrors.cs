using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Payments.Domain.CardPayments;

public static class CardPaymentErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("payments.card_payment_not_found", $"Card payment '{id}' was not found.");

    public static Error NotAuthorized(Guid id, CardPaymentStatus status) =>
        Error.Conflict(
            "payments.card_payment_not_authorized",
            $"Card payment '{id}' is {status}, not authorized."
        );

    public static Error AmountMismatch(Guid id, decimal authorized, decimal requested) =>
        Error.Conflict(
            "payments.card_payment_amount_mismatch",
            $"Card payment '{id}' authorized {authorized}, the order needs {requested}."
        );
}
