namespace OrderFlow.Payments.Application.Deposits.GetDepositSagas;

public sealed record DepositSagaDto(
    Guid DepositId,
    string State,
    decimal Amount,
    string Currency,
    string CardBrand,
    string CardLast4,
    string? FailureReason,
    DateTimeOffset StartedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<DepositSagaStepDto> Steps
);

public sealed record DepositSagaStepDto(string Name, DateTimeOffset OccurredAt, string? Detail);
