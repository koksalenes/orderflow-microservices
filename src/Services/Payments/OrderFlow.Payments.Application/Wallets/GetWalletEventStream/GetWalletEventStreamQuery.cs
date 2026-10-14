namespace OrderFlow.Payments.Application.Wallets.GetWalletEventStream;

public sealed record GetWalletEventStreamQuery(string OwnerId)
{
    public const int MaxEvents = 50;
}
