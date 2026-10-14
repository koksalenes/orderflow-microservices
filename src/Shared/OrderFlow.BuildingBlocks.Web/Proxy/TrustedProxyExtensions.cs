using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using IPNetwork = System.Net.IPNetwork;

namespace OrderFlow.BuildingBlocks.Web.Proxy;

internal static class TrustedProxyExtensions
{
    public static IServiceCollection AddTrustedProxy(this IServiceCollection services)
    {
        services
            .AddOptions<TrustedProxyOptions>()
            .BindConfiguration(TrustedProxyOptions.SectionName)
            .Validate(
                options => options.IsValid(),
                "TrustedProxy: every KnownNetworks entry must be a CIDR range and every KnownProxies entry an IP address."
            )
            .ValidateOnStart();

        services
            .AddOptions<ForwardedHeadersOptions>()
            .Configure<IOptions<TrustedProxyOptions>>(
                (forwarded, trusted) =>
                {
                    forwarded.ForwardedHeaders =
                        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                    foreach (var network in trusted.Value.KnownNetworks)
                    {
                        forwarded.KnownIPNetworks.Add(IPNetwork.Parse(network));
                    }

                    foreach (var proxy in trusted.Value.KnownProxies)
                    {
                        forwarded.KnownProxies.Add(IPAddress.Parse(proxy));
                    }
                }
            );

        return services;
    }
}
