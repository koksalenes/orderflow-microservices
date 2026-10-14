using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrderFlow.BuildingBlocks.Web.Versioning;

/**
 * URL segment versioning (/api/v1/...). Version 1 is the default, and every response
 * reports the supported versions in the api-supported-versions header.
 */
public static class ApiVersioningExtensions
{
    private static readonly ApiVersion DefaultVersion = new(1);

    internal static IServiceCollection AddApiVersioningDefaults(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = DefaultVersion;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        return services;
    }

    /**
     * Creates the route group for one resource: /api/v{version}/{resource}, version 1.
     * Endpoints of a feature are mapped on the returned group.
     */
    public static RouteGroupBuilder MapVersionedGroup(
        this IEndpointRouteBuilder app,
        string resource
    )
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.NewVersionedApi(resource)
            .MapGroup($"/api/v{{version:apiVersion}}/{resource}")
            .HasApiVersion(DefaultVersion);
    }
}
