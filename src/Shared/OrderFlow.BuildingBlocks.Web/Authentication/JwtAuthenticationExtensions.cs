using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrderFlow.BuildingBlocks.Web.Authentication;

public static class JwtAuthenticationExtensions
{
    public const string SubjectClaim = "sub";
    public const string RolesClaim = "roles";

    public static WebApplicationBuilder AddJwtAuthentication(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .Services.AddOptions<JwtAuthenticationOptions>()
            .BindConfiguration(JwtAuthenticationOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        builder
            .Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtAuthenticationOptions>>(
                (jwt, settings) =>
                {
                    var options = settings.Value;
                    jwt.Authority = options.Authority;
                    jwt.MetadataAddress = options.MetadataAddress ?? string.Empty;
                    jwt.Audience = options.Audience;
                    jwt.RequireHttpsMetadata = options.RequireHttpsMetadata;
                    jwt.MapInboundClaims = false;
                    jwt.TokenValidationParameters.ValidIssuer = options.Authority;
                    jwt.TokenValidationParameters.NameClaimType = "preferred_username";
                    jwt.TokenValidationParameters.RoleClaimType = RolesClaim;
                }
            );

        builder
            .Services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return builder;
    }

    public static WebApplication UseJwtAuthentication(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
