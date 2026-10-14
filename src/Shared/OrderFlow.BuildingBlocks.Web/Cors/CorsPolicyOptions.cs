namespace OrderFlow.BuildingBlocks.Web.Cors;

public sealed class CorsPolicyOptions
{
    public const string SectionName = "Cors";

    /** Exact origins such as "http://localhost:5173"; wildcards are not allowed. */
    public IReadOnlyList<string> AllowedOrigins { get; init; } = [];

    internal bool IsValid() =>
        AllowedOrigins.All(origin =>
            Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && uri.AbsolutePath == "/"
            && !origin.EndsWith('/')
        );
}
