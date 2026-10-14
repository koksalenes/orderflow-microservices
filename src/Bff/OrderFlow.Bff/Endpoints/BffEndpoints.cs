using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OrderFlow.Bff.Authentication;
using OrderFlow.Bff.Session;
using OrderFlow.BuildingBlocks.Web.Errors;

namespace OrderFlow.Bff.Endpoints;

internal static class BffEndpoints
{
#pragma warning disable S5332
    private const string BackchannelLogoutEvent =
        "http://schemas.openid.net/event/backchannel-logout";
#pragma warning restore S5332

    public static IEndpointRouteBuilder MapBffEndpoints(this IEndpointRouteBuilder app)
    {
        var bff = app.MapGroup("/bff").WithTags("BFF");

        bff.MapGet("/login", Login)
            .WithName("BffLogin")
            .WithSummary("Starts the Keycloak sign-in and returns to a local URL.");
        bff.MapGet("/logout", Logout)
            .WithName("BffLogout")
            .WithSummary("Ends the BFF session and the Keycloak session.");
        bff.MapGet("/user", (Delegate)GetUser)
            .WithName("BffUser")
            .WithSummary("Returns the signed-in user, or 401.");
        bff.MapPost("/backchannel-logout", BackchannelLogout)
            .WithName("BffBackchannelLogout")
            .WithSummary("Keycloak calls this when a session ends elsewhere.")
            .DisableAntiforgery();

        return app;
    }

    private static ChallengeHttpResult Login(string? returnUrl, string? prompt, string? locale)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = IsLocalUrl(returnUrl) ? returnUrl : "/",
        };

        if (string.Equals(prompt, "create", StringComparison.Ordinal))
        {
            properties.Items[BffAuthenticationDefaults.RegisterItem] = "true";
        }

        if (locale is not null && BffAuthenticationDefaults.SupportedLocales.Contains(locale))
        {
            properties.Items[BffAuthenticationDefaults.LocaleItem] = locale;
        }

        return TypedResults.Challenge(properties, [BffAuthenticationDefaults.OidcScheme]);
    }

    private static async Task<IResult> Logout(HttpContext context, string? sid)
    {
        var result = await context
            .AuthenticateAsync(BffAuthenticationDefaults.CookieScheme)
            .ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return TypedResults.LocalRedirect("/");
        }

        var sessionId = result.Principal.FindFirstValue("sid");
        return sessionId is not null && !string.Equals(sessionId, sid, StringComparison.Ordinal)
            ? Problem(
                StatusCodes.Status400BadRequest,
                "The logout request is invalid.",
                ErrorCodes.InvalidLogoutRequest
            )
            : TypedResults.SignOut(
                new AuthenticationProperties { RedirectUri = "/" },
                [BffAuthenticationDefaults.CookieScheme, BffAuthenticationDefaults.OidcScheme]
            );
    }

    private static async Task<IResult> GetUser(HttpContext context)
    {
        var result = await context
            .AuthenticateAsync(BffAuthenticationDefaults.CookieScheme)
            .ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return Problem(
                StatusCodes.Status401Unauthorized,
                "You are not signed in.",
                ErrorCodes.Unauthorized
            );
        }

        var user = result.Principal;
        var sessionId = user.FindFirstValue("sid") ?? string.Empty;

        return TypedResults.Ok(
            new BffUser(
                Subject: user.FindFirstValue("sub") ?? string.Empty,
                Name: user.FindFirstValue("name") ?? user.FindFirstValue("email") ?? string.Empty,
                GivenName: user.FindFirstValue("given_name"),
                FamilyName: user.FindFirstValue("family_name"),
                Email: user.FindFirstValue("email"),
                EmailVerified: string.Equals(
                    user.FindFirstValue("email_verified"),
                    "true",
                    StringComparison.OrdinalIgnoreCase
                ),
                Roles:
                [
                    .. user.FindAll("roles")
                        .Select(claim => claim.Value)
                        .Where(IsApplicationRole)
                        .Distinct(StringComparer.Ordinal),
                ],
                LogoutUrl: $"/bff/logout?sid={Uri.EscapeDataString(sessionId)}",
                ExpiresAt: result.Properties?.ExpiresUtc
            )
        );
    }

    private static async Task<IResult> BackchannelLogout(
        HttpContext context,
        IOptionsMonitor<OpenIdConnectOptions> oidcOptions,
        RedisTicketStore ticketStore
    )
    {
        var form = await context
            .Request.ReadFormAsync(context.RequestAborted)
            .ConfigureAwait(false);
        var logoutToken = form["logout_token"].ToString();
        if (string.IsNullOrEmpty(logoutToken))
        {
            return Problem(
                StatusCodes.Status400BadRequest,
                "logout_token is missing.",
                ErrorCodes.InvalidLogoutRequest
            );
        }

        var options = oidcOptions.Get(BffAuthenticationDefaults.OidcScheme);
        var configuration = await options
            .ConfigurationManager!.GetConfigurationAsync(context.RequestAborted)
            .ConfigureAwait(false);

        var validation = await new JsonWebTokenHandler()
            .ValidateTokenAsync(
                logoutToken,
                new TokenValidationParameters
                {
                    ValidIssuer = configuration.Issuer,
                    ValidAudience = options.ClientId,
                    IssuerSigningKeys = configuration.SigningKeys,
                }
            )
            .ConfigureAwait(false);

        if (
            !validation.IsValid
            || validation.SecurityToken is not JsonWebToken token
            || token.TryGetPayloadValue<object>("nonce", out _)
            || !token.TryGetPayloadValue<Dictionary<string, object>>("events", out var events)
            || !events.ContainsKey(BackchannelLogoutEvent)
            || !token.TryGetPayloadValue<string>("sid", out var sessionId)
        )
        {
            return Problem(
                StatusCodes.Status400BadRequest,
                "The logout token is invalid.",
                ErrorCodes.InvalidLogoutRequest
            );
        }

        await ticketStore
            .RemoveBySessionIdAsync(sessionId, context.RequestAborted)
            .ConfigureAwait(false);
        context.Response.Headers.CacheControl = "no-store";
        return TypedResults.Ok();
    }

    private static bool IsApplicationRole(string role) =>
        !role.StartsWith("default-roles-", StringComparison.Ordinal)
        && role is not "offline_access" and not "uma_authorization";

    private static bool IsLocalUrl(string? url) =>
        !string.IsNullOrEmpty(url)
        && url[0] == '/'
        && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'));

    private static ProblemHttpResult Problem(int status, string title, string code) =>
        TypedResults.Problem(
            title: title,
            statusCode: status,
            extensions: new Dictionary<string, object?> { ["code"] = code }
        );
}

internal sealed record BffUser(
    string Subject,
    string Name,
    string? GivenName,
    string? FamilyName,
    string? Email,
    bool EmailVerified,
    IReadOnlyList<string> Roles,
    string LogoutUrl,
    DateTimeOffset? ExpiresAt
);
