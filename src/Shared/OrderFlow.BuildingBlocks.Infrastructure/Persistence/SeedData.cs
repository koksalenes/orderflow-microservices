using System.Reflection;
using System.Text.Json;

namespace OrderFlow.BuildingBlocks.Infrastructure.Persistence;

public static class SeedData
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<IReadOnlyList<T>> ReadAsync<T>(
        Assembly assembly,
        string resourceName,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var stream =
            assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded seed file '{resourceName}' was not found in {assembly.GetName().Name}."
            );

        await using (stream.ConfigureAwait(false))
        {
            return await JsonSerializer
                    .DeserializeAsync<List<T>>(stream, JsonOptions, cancellationToken)
                    .ConfigureAwait(false)
                ?? [];
        }
    }
}
