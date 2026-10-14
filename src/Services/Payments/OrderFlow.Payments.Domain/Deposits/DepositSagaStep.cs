namespace OrderFlow.Payments.Domain.Deposits;

public sealed record DepositSagaStep(string Name, DateTimeOffset OccurredAt, string? Detail = null);
