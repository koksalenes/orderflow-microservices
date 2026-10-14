using System.Text.RegularExpressions;

namespace OrderFlow.BuildingBlocks.ServiceDefaults.Correlation;

public static partial class CorrelationId
{
    public const string HeaderName = "X-Correlation-Id";
    public const string LogPropertyName = "CorrelationId";
    public const string TagName = "correlation.id";

    public static bool IsValid(string? value) => value is not null && ValidFormat().IsMatch(value);

    public static string New(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        return Guid.CreateVersion7(timeProvider.GetUtcNow()).ToString("N");
    }

    [GeneratedRegex("^[A-Za-z0-9._-]{1,64}$")]
    private static partial Regex ValidFormat();
}
