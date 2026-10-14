using System.Globalization;

namespace OrderFlow.Notifications.Application.Emails;

public static class EmailFormat
{
    public static string Money(decimal amount, string currency, string locale)
    {
        var number = amount.ToString("#,##0.00", CultureInfo.InvariantCulture);
        if (locale == "tr")
        {
            number = number.Replace(',', ' ').Replace('.', ',').Replace(' ', '.');
        }

        return currency switch
        {
            "USD" => $"${number}",
            "EUR" => $"€{number}",
            "TRY" => $"₺{number}",
            _ => $"{number} {currency}",
        };
    }

    public static string DateTime(DateTimeOffset value, string locale) =>
        value
            .ToUniversalTime()
            .ToString(
                locale == "tr" ? "dd.MM.yyyy HH:mm 'UTC'" : "MMM d, yyyy, HH:mm 'UTC'",
                CultureInfo.InvariantCulture
            );
}
