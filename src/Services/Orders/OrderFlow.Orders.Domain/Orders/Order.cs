using OrderFlow.BuildingBlocks.Domain;

namespace OrderFlow.Orders.Domain.Orders;

public sealed class Order : AggregateRoot<Guid>
{
    private readonly List<OrderLine> _lines = [];
    private readonly List<OrderStep> _steps = [];

    private Order() { }

    public string Number { get; private set; } = string.Empty;

    public string CustomerId { get; private set; } = string.Empty;

    public string? CustomerEmail { get; private set; }

    public string? CustomerName { get; private set; }

    public string Locale { get; private set; } = string.Empty;

    public IReadOnlyList<OrderLine> Lines => _lines;

    public ShippingAddress ShippingAddress { get; private set; } = null!;

    public string Currency { get; private set; } = string.Empty;

    public decimal Subtotal { get; private set; }

    public decimal ShippingFee { get; private set; }

    public decimal Total { get; private set; }

    public PaymentMethod PaymentMethod { get; private set; }

    public Guid? CardPaymentId { get; private set; }

    public string? CardBrand { get; private set; }

    public string? CardLast4 { get; private set; }

    public string? Scenario { get; private set; }

    public OrderStatus Status { get; private set; }

    public string? CancellationReason { get; private set; }

    public string? PaymentReference { get; private set; }

    public string? TrackingNumber { get; private set; }

    public string? InvoiceNumber { get; private set; }

    public Guid? RefundId { get; private set; }

    public DateTimeOffset PlacedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? NextActionAt { get; private set; }

    public IReadOnlyList<OrderStep> Steps => _steps;

    public uint Version { get; }

    public static Order Place(
        Guid id,
        string customerId,
        string? customerEmail,
        string? customerName,
        string locale,
        IEnumerable<OrderLine> lines,
        ShippingAddress shippingAddress,
        string currency,
        PaymentMethod paymentMethod,
        Guid? cardPaymentId,
        string? cardBrand,
        string? cardLast4,
        string? scenario,
        DateTimeOffset now
    )
    {
        var order = new Order
        {
            Id = id,
            Number = $"OF-{id.ToString("N")[^8..].ToUpperInvariant()}",
            CustomerId = customerId,
            CustomerEmail = customerEmail,
            CustomerName = customerName,
            Locale = locale,
            ShippingAddress = shippingAddress,
            Currency = currency,
            PaymentMethod = paymentMethod,
            CardPaymentId = cardPaymentId,
            CardBrand = cardBrand,
            CardLast4 = cardLast4,
            Scenario = scenario,
            Status = OrderStatus.Placed,
            PlacedAt = now,
        };
        order._lines.AddRange(lines);
        order.Subtotal = order._lines.Sum(line => line.LineTotal);
        order.ShippingFee = 0;
        order.Total = order.Subtotal + order.ShippingFee;
        order.Record(OrderSteps.Placed, now, order.Number);
        return order;
    }

    public bool MarkStockReserved(DateTimeOffset now)
    {
        if (Status != OrderStatus.Placed)
        {
            return false;
        }

        Status = OrderStatus.AwaitingPayment;
        Record(OrderSteps.StockReserved, now);
        Record(OrderSteps.PaymentRequested, now, PaymentMethod.ToString().ToUpperInvariant());
        return true;
    }

    public bool RejectStock(string reason, string? detail, DateTimeOffset now)
    {
        if (Status != OrderStatus.Placed)
        {
            return false;
        }

        Record(OrderSteps.StockReservationFailed, now, detail);
        if (PaymentMethod == PaymentMethod.Card)
        {
            Record(OrderSteps.AuthorizationVoided, now);
        }

        Cancel(reason, now);
        return true;
    }

    public bool MarkPaid(string reference, DateTimeOffset now, DateTimeOffset confirmAt)
    {
        if (Status != OrderStatus.AwaitingPayment)
        {
            return false;
        }

        Status = OrderStatus.Paid;
        PaymentReference = reference;
        NextActionAt = confirmAt;
        Record(OrderSteps.PaymentCaptured, now, reference);
        return true;
    }

    public bool FailPayment(string reason, DateTimeOffset now)
    {
        if (Status != OrderStatus.AwaitingPayment)
        {
            return false;
        }

        Record(OrderSteps.PaymentFailed, now, reason);
        Record(OrderSteps.StockReleased, now);
        Cancel(reason, now);
        return true;
    }

    public bool Confirm(DateTimeOffset now, DateTimeOffset shipAt) =>
        Advance(OrderStatus.Paid, OrderStatus.Confirmed, OrderSteps.Confirmed, now, shipAt);

    public bool Ship(string trackingNumber, DateTimeOffset now, DateTimeOffset deliverAt)
    {
        if (
            !Advance(
                OrderStatus.Confirmed,
                OrderStatus.Shipped,
                OrderSteps.Shipped,
                now,
                deliverAt,
                trackingNumber
            )
        )
        {
            return false;
        }

        TrackingNumber = trackingNumber;
        Record(OrderSteps.StockCommitted, now);
        return true;
    }

    public bool Deliver(DateTimeOffset now, DateTimeOffset invoiceAt) =>
        Advance(OrderStatus.Shipped, OrderStatus.Delivered, OrderSteps.Delivered, now, invoiceAt);

    public bool FailDelivery(Guid refundId, DateTimeOffset now)
    {
        if (Status != OrderStatus.Shipped)
        {
            return false;
        }

        Status = OrderStatus.Refunding;
        RefundId = refundId;
        NextActionAt = null;
        Record(OrderSteps.DeliveryFailed, now, TrackingNumber);
        Record(OrderSteps.RefundRequested, now);
        return true;
    }

    public bool MarkRefunded(DateTimeOffset now)
    {
        if (Status != OrderStatus.Refunding)
        {
            return false;
        }

        Status = OrderStatus.Refunded;
        CancellationReason = OrderScenarios.LostParcel;
        Record(OrderSteps.Refunded, now);
        return true;
    }

    public bool Invoice(string invoiceNumber, DateTimeOffset now)
    {
        if (
            !Advance(
                OrderStatus.Delivered,
                OrderStatus.Completed,
                OrderSteps.Invoiced,
                now,
                null,
                invoiceNumber
            )
        )
        {
            return false;
        }

        InvoiceNumber = invoiceNumber;
        return true;
    }

    private bool Advance(
        OrderStatus from,
        OrderStatus to,
        string step,
        DateTimeOffset now,
        DateTimeOffset? nextActionAt,
        string? detail = null
    )
    {
        if (Status != from)
        {
            return false;
        }

        Status = to;
        NextActionAt = nextActionAt;
        Record(step, now, detail);
        return true;
    }

    private void Cancel(string reason, DateTimeOffset now)
    {
        Status = OrderStatus.Cancelled;
        CancellationReason = reason;
        NextActionAt = null;
        Record(OrderSteps.Cancelled, now, reason);
    }

    private void Record(string step, DateTimeOffset now, string? detail = null)
    {
        _steps.Add(new OrderStep(step, now, detail));
        UpdatedAt = now;
    }
}
