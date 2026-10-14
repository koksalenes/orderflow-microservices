namespace OrderFlow.Payments.Application.Wallets.DepositFunds;

public sealed record ReceiptRecipient(string Email, string? Name, string Locale)
{
    public const string DefaultLocale = "en";

    private static readonly HashSet<string> SupportedLocales = new(StringComparer.Ordinal)
    {
        "en",
        "tr",
    };

    public static string NormalizeLocale(string? locale) =>
        locale is not null && SupportedLocales.Contains(locale) ? locale : DefaultLocale;
}
