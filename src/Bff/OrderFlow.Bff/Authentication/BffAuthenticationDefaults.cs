using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace OrderFlow.Bff.Authentication;

internal static class BffAuthenticationDefaults
{
    public const string CookieScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    public const string OidcScheme = OpenIdConnectDefaults.AuthenticationScheme;

    public static readonly IReadOnlySet<string> SupportedLocales = new HashSet<string>(
        StringComparer.Ordinal
    )
    {
        "en",
        "tr",
    };

    public const string LocaleItem = "ui_locales";
    public const string RegisterItem = "register";
}
