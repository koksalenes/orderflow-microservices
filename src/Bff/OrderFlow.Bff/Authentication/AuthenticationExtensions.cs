using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using OrderFlow.Bff.Session;

namespace OrderFlow.Bff.Authentication;

internal static class AuthenticationExtensions
{
    public static IServiceCollection AddBffAuthentication(this IServiceCollection services)
    {
        services
            .AddOptions<OidcSettings>()
            .BindConfiguration(OidcSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient(TokenRefresher.HttpClientName);
        services.AddSingleton<TokenRefresher>();
        services.AddScoped<BffCookieEvents>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = BffAuthenticationDefaults.CookieScheme;
                options.DefaultChallengeScheme = BffAuthenticationDefaults.OidcScheme;
                options.DefaultSignOutScheme = BffAuthenticationDefaults.OidcScheme;
            })
            .AddCookie(BffAuthenticationDefaults.CookieScheme)
            .AddOpenIdConnect(BffAuthenticationDefaults.OidcScheme, _ => { });

        services
            .AddOptions<CookieAuthenticationOptions>(BffAuthenticationDefaults.CookieScheme)
            .Configure<IOptions<BffSessionOptions>, RedisTicketStore>(
                (options, session, ticketStore) =>
                {
                    options.Cookie.Name = session.Value.CookieName;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.ExpireTimeSpan = TimeSpan.FromHours(session.Value.LifetimeHours);
                    options.SlidingExpiration = true;
                    options.SessionStore = ticketStore;
                    options.EventsType = typeof(BffCookieEvents);
                }
            );

        services
            .AddOptions<OpenIdConnectOptions>(BffAuthenticationDefaults.OidcScheme)
            .Configure<IOptions<OidcSettings>>(
                (options, settings) => Configure(options, settings.Value)
            );

        return services;
    }

    private static void Configure(OpenIdConnectOptions options, OidcSettings settings)
    {
        options.Authority = settings.Authority;
        if (!string.IsNullOrWhiteSpace(settings.MetadataAddress))
        {
            options.MetadataAddress = settings.MetadataAddress;
        }
        options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
        options.ClientId = settings.ClientId;
        options.ClientSecret = settings.ClientSecret;

        options.ResponseType = "code";
        options.UsePkce = true;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.MapInboundClaims = false;
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "roles";
        options.SignedOutRedirectUri = "/";

        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.NonceCookie.SameSite = SameSiteMode.Lax;

        options.Events.OnRedirectToIdentityProvider = context =>
        {
            if (
                context.Properties.Items.TryGetValue(
                    BffAuthenticationDefaults.LocaleItem,
                    out var locale
                )
            )
            {
                context.ProtocolMessage.UiLocales = locale;
            }
            if (context.Properties.Items.ContainsKey(BffAuthenticationDefaults.RegisterItem))
            {
                context.ProtocolMessage.IssuerAddress =
                    context.ProtocolMessage.IssuerAddress.Replace(
                        "/protocol/openid-connect/auth",
                        "/protocol/openid-connect/registrations",
                        StringComparison.Ordinal
                    );
            }
            return Task.CompletedTask;
        };
    }
}
