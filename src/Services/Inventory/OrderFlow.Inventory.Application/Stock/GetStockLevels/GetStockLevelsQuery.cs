namespace OrderFlow.Inventory.Application.Stock.GetStockLevels;

/** Stock of several products at once (one catalog page). Unknown product ids are left out of the result. */
public sealed record GetStockLevelsQuery(IReadOnlyList<Guid> ProductIds)
{
    public const int MaxProductIds = 100;
}
