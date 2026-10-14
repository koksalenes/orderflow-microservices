using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Orders.Infrastructure.Fulfillment;

public sealed class FulfillmentOptions
{
    public const string SectionName = "Fulfillment";

    [Range(0, 3600)]
    public int ConfirmAfterSeconds { get; init; } = 5;

    [Range(0, 3600)]
    public int ShipAfterSeconds { get; init; } = 8;

    [Range(0, 3600)]
    public int DeliverAfterSeconds { get; init; } = 8;

    [Range(0, 3600)]
    public int InvoiceAfterSeconds { get; init; } = 3;

    [Range(200, 60_000)]
    public int PollIntervalMilliseconds { get; init; } = 1000;
}

public sealed class ServicesOptions
{
    public const string SectionName = "Services";

    [Required]
    public Uri ProductsGrpc { get; init; } = new("http://localhost:5121");
}
