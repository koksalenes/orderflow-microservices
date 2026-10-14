namespace OrderFlow.Notifications.Application.Emails;

public sealed record StorefrontLinks(Uri BaseUrl)
{
    public Uri Wallet => new(BaseUrl, "/wallet");
}
