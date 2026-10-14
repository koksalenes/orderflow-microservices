namespace OrderFlow.Notifications.Application.Emails;

public sealed record StorefrontLinks(Uri BaseUrl)
{
    public Uri Wallet => new(BaseUrl, "/wallet");

    public Uri Order(Guid orderId) => new(BaseUrl, $"/orders/{orderId}");
}
