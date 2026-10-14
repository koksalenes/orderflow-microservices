using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrderFlow.BuildingBlocks.Web.Cors;

internal static class CorsExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services
            .AddOptions<CorsPolicyOptions>()
            .BindConfiguration(CorsPolicyOptions.SectionName)
            .Validate(
                options => options.IsValid(),
                "Cors:AllowedOrigins must contain exact origins such as \"https://app.example.com\" (no path, no trailing slash, no wildcard)."
            )
            .ValidateOnStart();

        services.AddCors();

        services
            .AddOptions<CorsOptions>()
            .Configure<IOptions<CorsPolicyOptions>>(
                (cors, policy) =>
                    cors.AddDefaultPolicy(builder =>
                        builder
                            .WithOrigins([.. policy.Value.AllowedOrigins])
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .WithExposedHeaders("api-supported-versions")
                    )
            );

        return services;
    }
}
