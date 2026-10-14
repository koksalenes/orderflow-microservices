namespace OrderFlow.Inventory.Application.Stock;

/** Quantity a customer can still order. On-hand and reserved counts stay internal. */
public sealed record StockLevelDto(Guid ProductId, int Available);
