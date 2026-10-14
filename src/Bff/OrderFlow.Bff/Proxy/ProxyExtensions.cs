using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Yarp.ReverseProxy.Transforms;

namespace OrderFlow.Bff.Proxy;

internal static class ProxyExtensions
{
    public static IServiceCollection AddBffProxy(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddTransforms(context =>
                context.AddRequestTransform(async transform =>
                {
                    transform.ProxyRequest.Headers.Remove("Cookie");

                    var accessToken = await transform
                        .HttpContext.GetTokenAsync("access_token")
                        .ConfigureAwait(false);
                    if (accessToken is not null)
                    {
                        transform.ProxyRequest.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", accessToken);
                    }
                })
            );

        return services;
    }
}
