using Microsoft.AspNetCore.Http;

namespace OrderFlow.BuildingBlocks.Web;

public static class HttpContextExtensions
{
    public static string? ClientIp(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var address = context.Connection.RemoteIpAddress;
        return address is { IsIPv4MappedToIPv6: true }
            ? address.MapToIPv4().ToString()
            : address?.ToString();
    }
}
