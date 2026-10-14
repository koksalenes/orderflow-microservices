using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using OrderFlow.BuildingBlocks.Web;

namespace OrderFlow.Gateway.RateLimiting;

internal static class GatewayRateLimitingExtensions
{
    public static IServiceCollection AddGatewayRateLimiting(this IServiceCollection services)
    {
        services
            .AddOptions<GatewayRateLimitOptions>()
            .BindConfiguration(GatewayRateLimitOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = WriteRejectionAsync;
        });

        services
            .AddOptions<RateLimiterOptions>()
            .Configure<IOptions<GatewayRateLimitOptions>>(
                (options, limits) =>
                    options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                        AllRequests(limits.Value),
                        Writes(limits.Value),
                        Concurrency(limits.Value)
                    )
            );

        return services;
    }

    private static PartitionedRateLimiter<HttpContext> AllRequests(
        GatewayRateLimitOptions limits
    ) =>
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
            IsExempt(context)
                ? RateLimitPartition.GetNoLimiter(string.Empty)
                : RateLimitPartition.GetTokenBucketLimiter(
                    ClientKey(context),
                    _ => TokenBucket(limits.PermitsPerSecond, limits.Burst)
                )
        );

    private static PartitionedRateLimiter<HttpContext> Writes(GatewayRateLimitOptions limits) =>
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
            IsExempt(context) || IsRead(context.Request.Method)
                ? RateLimitPartition.GetNoLimiter(string.Empty)
                : RateLimitPartition.GetTokenBucketLimiter(
                    ClientKey(context),
                    _ => TokenBucket(limits.WritePermitsPerSecond, limits.WriteBurst)
                )
        );

    private static PartitionedRateLimiter<HttpContext> Concurrency(
        GatewayRateLimitOptions limits
    ) =>
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
            IsExempt(context)
                ? RateLimitPartition.GetNoLimiter(string.Empty)
                : RateLimitPartition.GetConcurrencyLimiter(
                    ClientKey(context),
                    _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = limits.ConcurrentRequests,
                        QueueLimit = 0,
                    }
                )
        );

    private static TokenBucketRateLimiterOptions TokenBucket(int permitsPerSecond, int burst) =>
        new()
        {
            TokenLimit = burst,
            TokensPerPeriod = permitsPerSecond,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            QueueLimit = 0,
            AutoReplenishment = true,
        };

    private static string ClientKey(HttpContext context) => context.ClientIp() ?? "unknown";

    private static bool IsExempt(HttpContext context) =>
        context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase);

    private static bool IsRead(string method) =>
        HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method);

    private static async ValueTask WriteRejectionAsync(
        OnRejectedContext context,
        CancellationToken cancellationToken
    )
    {
        var http = context.HttpContext;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            http.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds)
                .ToString(CultureInfo.InvariantCulture);
        }

        var problemDetails = http.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetails
            .WriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = http,
                    ProblemDetails = new ProblemDetails
                    {
                        Type = "https://tools.ietf.org/html/rfc6585#section-4",
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Too many requests.",
                    },
                }
            )
            .ConfigureAwait(false);
    }
}
