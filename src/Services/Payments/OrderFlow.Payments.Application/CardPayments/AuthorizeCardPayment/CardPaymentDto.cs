namespace OrderFlow.Payments.Application.CardPayments.AuthorizeCardPayment;

public sealed record CardPaymentDto(
    Guid Id,
    decimal Amount,
    string Currency,
    string CardBrand,
    string CardLast4,
    string AuthorizationCode,
    string Status
);
