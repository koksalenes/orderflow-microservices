using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Inventory.Domain.Stock;

public sealed class StockItem : AggregateRoot<Guid>
{
    private StockItem() { }

    public int OnHand { get; private set; }

    public int Reserved { get; }

    public int Available => OnHand - Reserved;

    /** Optimistic concurrency token (PostgreSQL xmin). */
    public uint Version { get; }

    public static StockItem Create(Guid productId, int onHand)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(onHand);

        return new StockItem { Id = productId, OnHand = onHand };
    }
}
