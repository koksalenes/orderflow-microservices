namespace OrderFlow.Payments.Application.Deposits.GetDepositSagas;

public sealed record GetDepositSagasQuery(string OwnerId, int Limit = 5)
{
    public const int MaxLimit = 20;
}
