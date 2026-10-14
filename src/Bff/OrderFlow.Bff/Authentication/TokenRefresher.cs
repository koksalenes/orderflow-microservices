using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace OrderFlow.Bff.Authentication;

internal sealed partial class TokenRefresher(
    IHttpClientFactory httpClientFactory,
    IOptionsMonitor<OpenIdConnectOptions> oidcOptions,
    ILogger<TokenRefresher> logger
)
{
    public const string HttpClientName = "oidc-backchannel";

    public async Task<TokenResponse?> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken
    )
    {
        var options = oidcOptions.Get(BffAuthenticationDefaults.OidcScheme);
        var configuration = await options
            .ConfigurationManager!.GetConfigurationAsync(cancellationToken)
            .ConfigureAwait(false);

        using var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = options.ClientId!,
                ["client_secret"] = options.ClientSecret!,
            }
        );

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var response = await client
            .PostAsync(new Uri(configuration.TokenEndpoint), content, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            LogRefreshFailed((int)response.StatusCode);
            return null;
        }

        return await response
            .Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
            .ConfigureAwait(false);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Token refresh rejected with status {StatusCode}; the session ends"
    )]
    private partial void LogRefreshFailed(int statusCode);
}

internal sealed record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("id_token")] string? IdToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
);
