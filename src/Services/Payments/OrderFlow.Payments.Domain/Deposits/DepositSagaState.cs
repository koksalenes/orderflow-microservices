namespace OrderFlow.Payments.Domain.Deposits;

public enum DepositSagaState
{
    AwaitingSettlement,
    Completed,
    Compensating,
    Compensated,
}
