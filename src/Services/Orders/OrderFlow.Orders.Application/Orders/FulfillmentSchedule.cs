namespace OrderFlow.Orders.Application.Orders;

public sealed record FulfillmentSchedule(
    TimeSpan ConfirmAfter,
    TimeSpan ShipAfter,
    TimeSpan DeliverAfter,
    TimeSpan InvoiceAfter
);
