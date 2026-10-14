using System.Globalization;

namespace OrderFlow.BuildingBlocks.ServiceDefaults.HealthChecks;

public static class HealthProbe
{
    private const string Command = "health-probe";
    private const string DefaultPath = "/health/live";
    private const string DefaultPort = "8080";

    public static bool IsRequested(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        return args.Length > 0 && string.Equals(args[0], Command, StringComparison.Ordinal);
    }

    public static async Task<int> RunAsync(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        var path = args.Length > 1 ? args[1] : DefaultPath;

        var port =
            Environment
                .GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS")
                ?.Split([';', ','], StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()
            ?? DefaultPort;

        var uri = new Uri(
            string.Create(CultureInfo.InvariantCulture, $"http://127.0.0.1:{port}{path}")
        );

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };

        try
        {
            using var response = await client.GetAsync(uri).ConfigureAwait(false);
            return response.IsSuccessStatusCode ? 0 : 1;
        }
        catch (Exception exception)
            when (exception is HttpRequestException or TaskCanceledException)
        {
            return 1;
        }
    }
}
