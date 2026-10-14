namespace OrderFlow.Inventory.Contracts.Reservations;

public sealed record StockLine(Guid ProductId, int Quantity);

public sealed record StockShortage(Guid ProductId, int Requested, int Available);

public sealed record ReserveStock(Guid OrderId, IReadOnlyList<StockLine> Lines)
{
    public const string MessageType = "inventory.reserve_stock.v1";
}

public sealed record StockReserved(Guid OrderId)
{
    public const string MessageType = "inventory.stock_reserved.v1";
}

public sealed record StockReservationFailed(
    Guid OrderId,
    string ReasonCode,
    IReadOnlyList<StockShortage> Shortages
)
{
    public const string MessageType = "inventory.stock_reservation_failed.v1";
}

public sealed record ReleaseStock(Guid OrderId, string ReasonCode)
{
    public const string MessageType = "inventory.release_stock.v1";
}

public sealed record CommitStock(Guid OrderId)
{
    public const string MessageType = "inventory.commit_stock.v1";
}
