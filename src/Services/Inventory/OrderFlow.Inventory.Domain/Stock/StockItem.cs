using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Inventory.Domain.Stock;

public sealed class StockItem : AggregateRoot<Guid>
{
    private StockItem() { }

    public int OnHand { get; private set; }

    public int Reserved { get; private set; }

    public int Available => OnHand - Reserved;

    public uint Version { get; }

    public static StockItem Create(Guid productId, int onHand)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(onHand);

        return new StockItem { Id = productId, OnHand = onHand };
    }

    public bool CanReserve(int quantity) => quantity > 0 && quantity <= Available;

    public void Reserve(int quantity)
    {
        if (!CanReserve(quantity))
        {
            throw new InvalidOperationException(
                $"Cannot reserve {quantity} of product '{Id}' ({Available} available)."
            );
        }

        Reserved += quantity;
    }

    public void Release(int quantity) => Reserved = Math.Max(0, Reserved - quantity);

    public void Commit(int quantity)
    {
        var shipped = Math.Min(quantity, Reserved);
        Reserved -= shipped;
        OnHand -= shipped;
    }
}
