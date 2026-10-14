using System.Globalization;
using System.Net;
using System.Text;
using OrderFlow.Notifications.Application.Emails;

namespace OrderFlow.Notifications.Application.Deposits;

internal static class WalletEmail
{
    private const string TemplateName =
        "OrderFlow.Notifications.Application.Deposits.Templates.wallet-receipt.html";

    private static readonly Lazy<string> Template = new(LoadTemplate);

    public static EmailMessage Create(WalletEmailModel model, StorefrontLinks links, int year)
    {
        var reversal = model.Kind == WalletEmailKind.Reversal;
        var texts = WalletEmailTexts.For(model.Kind, model.Locale);
        var amount = EmailFormat.Money(model.Amount, model.Currency, model.Locale);
        var balance = EmailFormat.Money(model.Balance, model.Currency, model.Locale);
        var card = $"{model.CardBrand} •••• {model.CardLast4}";
        var date = EmailFormat.DateTime(model.OccurredAt, model.Locale);
        var code = reversal ? WalletEmailTexts.Reason(model.Code, model.Locale) : model.Code;
        var reference = model.DepositId.ToString();
        var greeting = string.IsNullOrWhiteSpace(model.RecipientName)
            ? texts.GreetingNoName
            : texts.Greeting.Replace("{name}", model.RecipientName, StringComparison.Ordinal);
        string Fill(string text)
        {
            return text.Replace("{amount}", amount, StringComparison.Ordinal)
                .Replace("{balance}", balance, StringComparison.Ordinal);
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["lang"] = model.Locale,
            ["subject"] = Fill(texts.Subject),
            ["preheader"] = Fill(texts.Preheader),
            ["icon"] = reversal ? "↺" : "✓",
            ["iconBackground"] = reversal ? "#fee2e2" : "#dcfce7",
            ["iconColor"] = reversal ? "#b91c1c" : "#15803d",
            ["heading"] = texts.Heading,
            ["greeting"] = greeting,
            ["intro"] = texts.Intro,
            ["amountLabel"] = texts.AmountLabel,
            ["amountSign"] = reversal ? "−" : "+",
            ["amountColor"] = reversal ? "#b91c1c" : "#15803d",
            ["amount"] = amount,
            ["newBalanceLabel"] = texts.NewBalance,
            ["newBalance"] = balance,
            ["cardLabel"] = texts.Card,
            ["card"] = card,
            ["dateLabel"] = texts.Date,
            ["date"] = date,
            ["codeLabel"] = texts.CodeLabel,
            ["code"] = code,
            ["referenceLabel"] = texts.Reference,
            ["reference"] = reference,
            ["button"] = texts.Button,
            ["walletUrl"] = links.Wallet.ToString(),
            ["testNotice"] = texts.TestNotice,
            ["footer"] = texts.Footer,
            ["year"] = year.ToString(CultureInfo.InvariantCulture),
        };

        var html = new StringBuilder(Template.Value);
        foreach (var (key, value) in values)
        {
            html.Replace($"{{{{{key}}}}}", WebUtility.HtmlEncode(value));
        }

        var text = string.Join(
            Environment.NewLine,
            greeting,
            string.Empty,
            texts.Intro,
            string.Empty,
            $"{texts.AmountLabel}: {values["amountSign"]}{amount}",
            $"{texts.NewBalance}: {balance}",
            $"{texts.Card}: {card}",
            $"{texts.Date}: {date}",
            $"{texts.CodeLabel}: {code}",
            $"{texts.Reference}: {reference}",
            string.Empty,
            $"{texts.Button}: {links.Wallet}",
            string.Empty,
            texts.TestNotice,
            string.Empty,
            texts.Footer
        );

        return new EmailMessage(
            model.RecipientEmail,
            model.RecipientName,
            Fill(texts.Subject),
            html.ToString(),
            text,
            $"{(reversal ? "reversal" : "deposit")}-{reference}@orderflow.local"
        );
    }

    private static string LoadTemplate()
    {
        using var stream =
            typeof(WalletEmail).Assembly.GetManifestResourceStream(TemplateName)
            ?? throw new InvalidOperationException($"Email template '{TemplateName}' is missing.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
