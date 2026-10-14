using System.Text.Json;
using OrderFlow.Payments.Domain.EventSourcing;
using OrderFlow.Payments.Domain.Wallets;

namespace OrderFlow.Payments.Infrastructure.EventStore;

internal static class EventTypeRegistry
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly Dictionary<string, Type> TypesByName = new(StringComparer.Ordinal)
    {
        ["wallet.opened.v1"] = typeof(WalletOpened),
        ["wallet.funds_deposited.v1"] = typeof(FundsDeposited),
        ["wallet.deposit_settled.v1"] = typeof(DepositSettled),
        ["wallet.deposit_reversed.v1"] = typeof(DepositReversed),
    };

    private static readonly Dictionary<Type, string> NamesByType = TypesByName.ToDictionary(
        pair => pair.Value,
        pair => pair.Key
    );

    public static string NameOf(IDomainEvent domainEvent) =>
        NamesByType.TryGetValue(domainEvent.GetType(), out var name)
            ? name
            : throw new InvalidOperationException(
                $"Event '{domainEvent.GetType().Name}' is not registered in EventTypeRegistry."
            );

    public static string Serialize(IDomainEvent domainEvent) =>
        JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions);

    public static IDomainEvent Deserialize(string eventType, string data) =>
        TypesByName.TryGetValue(eventType, out var type)
            ? (IDomainEvent)(
                JsonSerializer.Deserialize(data, type, JsonOptions)
                ?? throw new InvalidOperationException($"Event '{eventType}' has no data.")
            )
            : throw new InvalidOperationException($"Unknown event type '{eventType}'.");

    public static string SerializeState<T>(T state) => JsonSerializer.Serialize(state, JsonOptions);

    public static T? DeserializeState<T>(string state) =>
        JsonSerializer.Deserialize<T>(state, JsonOptions);
}
