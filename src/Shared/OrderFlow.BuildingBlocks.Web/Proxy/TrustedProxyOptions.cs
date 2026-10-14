using System.Net;
using IPNetwork = System.Net.IPNetwork;

namespace OrderFlow.BuildingBlocks.Web.Proxy;

/**
 * Bound from the "TrustedProxy" section. X-Forwarded-* headers are accepted only from
 * these addresses/networks (plus loopback); from anyone else they are ignored.
 */
public sealed class TrustedProxyOptions
{
    public const string SectionName = "TrustedProxy";

    /** CIDR ranges, e.g. the Docker network the gateway runs in ("10.201.0.0/24"). */
    public IReadOnlyList<string> KnownNetworks { get; init; } = [];

    /** Single proxy addresses, e.g. "10.201.0.10". */
    public IReadOnlyList<string> KnownProxies { get; init; } = [];

    internal bool IsValid() =>
        KnownNetworks.All(network => IPNetwork.TryParse(network, out _))
        && KnownProxies.All(proxy => IPAddress.TryParse(proxy, out _));
}
