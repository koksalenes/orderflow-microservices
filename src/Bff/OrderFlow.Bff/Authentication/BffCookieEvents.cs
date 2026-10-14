using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace OrderFlow.Bff.Authentication;

internal sealed class BffCookieEvents(TokenRefresher tokenRefresher, TimeProvider timeProvider)
    : CookieAuthenticationEvents
{
    private static readonly TimeSpan RefreshBeforeExpiry = TimeSpan.FromSeconds(60);

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var properties = context.Properties;
        var expiresAt = properties.GetTokenValue("expires_at");
        var refreshToken = properties.GetTokenValue("refresh_token");

        if (
            expiresAt is null
            || refreshToken is null
            || DateTimeOffset.Parse(expiresAt, CultureInfo.InvariantCulture)
                - timeProvider.GetUtcNow()
                > RefreshBeforeExpiry
        )
        {
            return;
        }

        var tokens = await tokenRefresher
            .RefreshAsync(refreshToken, context.HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (tokens is null)
        {
            context.RejectPrincipal();
            await context
                .HttpContext.SignOutAsync(BffAuthenticationDefaults.CookieScheme)
                .ConfigureAwait(false);
            return;
        }

        properties.UpdateTokenValue("access_token", tokens.AccessToken);
        if (tokens.RefreshToken is not null)
        {
            properties.UpdateTokenValue("refresh_token", tokens.RefreshToken);
        }
        if (tokens.IdToken is not null)
        {
            properties.UpdateTokenValue("id_token", tokens.IdToken);
        }
        properties.UpdateTokenValue(
            "expires_at",
            timeProvider
                .GetUtcNow()
                .AddSeconds(tokens.ExpiresIn)
                .ToString("o", CultureInfo.InvariantCulture)
        );

        context.ShouldRenew = true;
    }

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    public override Task RedirectToAccessDenied(
        RedirectContext<CookieAuthenticationOptions> context
    )
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }
}
