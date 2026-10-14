using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Inventory.Domain.Reservations;

public sealed class StockReservation : AggregateRoot<Guid>
{
    private readonly List<ReservationLine> _lines = [];

    private StockReservation() { }

    public ReservationStatus Status { get; private set; }

    public IReadOnlyList<ReservationLine> Lines => _lines;

    public string? Reason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public uint Version { get; }

    public static StockReservation Reserved(
        Guid orderId,
        IEnumerable<ReservationLine> lines,
        DateTimeOffset now
    )
    {
        var reservation = new StockReservation
        {
            Id = orderId,
            Status = ReservationStatus.Held,
            CreatedAt = now,
            UpdatedAt = now,
        };
        reservation._lines.AddRange(lines);
        return reservation;
    }

    public static StockReservation Rejected(
        Guid orderId,
        IEnumerable<ReservationLine> lines,
        string reason,
        DateTimeOffset now
    )
    {
        var reservation = Reserved(orderId, lines, now);
        reservation.Status = ReservationStatus.Rejected;
        reservation.Reason = reason;
        return reservation;
    }

    public bool Release(string reason, DateTimeOffset now) =>
        Transition(ReservationStatus.Released, reason, now);

    public bool Commit(DateTimeOffset now) => Transition(ReservationStatus.Committed, null, now);

    private bool Transition(ReservationStatus to, string? reason, DateTimeOffset now)
    {
        if (Status != ReservationStatus.Held)
        {
            return false;
        }

        Status = to;
        Reason = reason;
        UpdatedAt = now;
        return true;
    }
}

public sealed record ReservationLine(Guid ProductId, int Quantity);

public enum ReservationStatus
{
    Held,
    Rejected,
    Released,
    Committed,
}
