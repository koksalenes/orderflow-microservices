using System.Globalization;
using System.Net;
using System.Text;
using OrderFlow.Notifications.Application.Emails;
using OrderFlow.Orders.Contracts.Orders;

namespace OrderFlow.Notifications.Application.Orders;

internal static class OrderEmail
{
    private const string TemplateName =
        "OrderFlow.Notifications.Application.Orders.Templates.order-update.html";

    private const decimal VatRate = 0.20m;

    private static readonly string[] Progress =
    [
        OrderStatuses.Placed,
        OrderStatuses.Confirmed,
        OrderStatuses.Shipped,
        OrderStatuses.Delivered,
        OrderStatuses.Invoiced,
    ];

    private static readonly Lazy<string> Template = new(LoadTemplate);

    public static EmailMessage? Create(OrderStatusChanged message, StorefrontLinks links, int year)
    {
        var order = message.Order;
        if (string.IsNullOrWhiteSpace(order.CustomerEmail))
        {
            return null;
        }

        var locale = order.Locale;
        var status = message.Status;
        var texts = OrderEmailCopy.For(status, locale);
        string L(string key)
        {
            return OrderEmailCopy.Label(key, locale);
        }

        string Money(decimal amount)
        {
            return EmailFormat.Money(amount, order.Currency, locale);
        }

        string E(string? value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }

        string Fill(string text)
        {
            return text.Replace("{number}", order.OrderNumber, StringComparison.Ordinal)
                        .Replace("{invoice}", order.InvoiceNumber ?? string.Empty, StringComparison.Ordinal)
                        .Replace(
                            "{reason}",
                            OrderEmailCopy.Reason(message.ReasonCode, locale),
                            StringComparison.Ordinal
                        );
        }

        var failed = status is OrderStatuses.Cancelled or OrderStatuses.Refunded;
        var (icon, accent, iconBackground) = status switch
        {
            OrderStatuses.Cancelled => ("✕", "#b91c1c", "#fee2e2"),
            OrderStatuses.Refunded => ("↺", "#b45309", "#fef3c7"),
            OrderStatuses.Shipped => ("➜", "#2563eb", "#dbeafe"),
            OrderStatuses.Invoiced => ("€", "#2563eb", "#dbeafe"),
            _ => ("✓", "#15803d", "#dcfce7"),
        };

        var greeting = string.IsNullOrWhiteSpace(order.CustomerName)
            ? L("greetingNoName")
            : L("greeting").Replace("{name}", order.CustomerName, StringComparison.Ordinal);

        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["lang"] = locale,
            ["subject"] = Fill(texts.Subject),
            ["preheader"] = Fill(texts.Intro),
            ["icon"] = icon,
            ["accent"] = accent,
            ["iconBackground"] = iconBackground,
            ["orderLabel"] = L("order"),
            ["orderNumber"] = order.OrderNumber,
            ["placedAt"] = EmailFormat.DateTime(order.PlacedAt, locale),
            ["heading"] = texts.Heading,
            ["greeting"] = greeting,
            ["intro"] = Fill(texts.Intro),
            ["itemsLabel"] = L("items"),
            ["addressLabel"] = status == OrderStatuses.Invoiced ? L("billedTo") : L("address"),
            ["paymentLabel"] = L("payment"),
            ["payment"] =
                order.PaymentMethod == "card"
                    ? $"{order.CardBrand ?? "Card"} •••• {order.CardLast4 ?? "????"}"
                    : L("wallet"),
            ["button"] = L("button"),
            ["orderUrl"] = links.Order(order.OrderId).ToString(),
            ["testNotice"] = L("test"),
            ["footer"] = L("footer"),
            ["year"] = year.ToString(CultureInfo.InvariantCulture),
        };

        var raw = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["progress"] = failed ? string.Empty : ProgressBar(status, L),
            ["highlight"] = Highlight(message, L),
            ["itemRows"] = string.Concat(
                order.Lines.Select(line =>
                    $"""
                        <tr>
                            <td class="thumb" width="56" style="padding:8px 12px 8px 0;border-bottom:1px solid #e2e8f0;">{(
                            line.ImageUrl is null
                                ? string.Empty
                                : $"<img src=\"{E(line.ImageUrl.ToString())}\" width=\"48\" height=\"48\" alt=\"\" style=\"display:block;border-radius:8px;border:1px solid #e2e8f0;background:#ffffff;\">"
                        )}</td>
                            <td style="padding:8px 0;border-bottom:1px solid #e2e8f0;color:#0f172a;">{E(
                            line.Name
                        )}<br><span style="color:#64748b;font-size:13px;">{line.Quantity} × {E(
                            Money(line.UnitPrice)
                        )}</span></td>
                            <td align="right" style="padding:8px 0;border-bottom:1px solid #e2e8f0;color:#0f172a;font-weight:600;white-space:nowrap;">{E(
                            Money(line.LineTotal)
                        )}</td>
                        </tr>
                        """
                )
            ),
            ["totalRows"] = TotalRows(order, status, L, Money, E),
            ["address"] = string.Join(
                "<br>",
                new[]
                {
                    order.ShippingAddress.FullName,
                    order.ShippingAddress.Line1,
                    order.ShippingAddress.Line2,
                    $"{order.ShippingAddress.PostalCode} {order.ShippingAddress.City}",
                    order.ShippingAddress.Country,
                    order.ShippingAddress.Phone,
                }
                    .Where(part => !string.IsNullOrWhiteSpace(part))
                    .Select(E)
            ),
        };

        var html = new StringBuilder(Template.Value);
        foreach (var (key, value) in values)
        {
            html.Replace($"{{{{{key}}}}}", E(value));
        }

        foreach (var (key, value) in raw)
        {
            html.Replace($"[[{key}]]", value);
        }

        var text = new StringBuilder()
            .AppendLine(greeting)
            .AppendLine()
            .AppendLine(Fill(texts.Intro))
            .AppendLine()
            .AppendLine(CultureInfo.InvariantCulture, $"{L("order")}: {order.OrderNumber}");
        foreach (var line in order.Lines)
        {
            text.AppendLine(
                CultureInfo.InvariantCulture,
                $"- {line.Quantity} × {line.Name}: {Money(line.LineTotal)}"
            );
        }

        text.AppendLine(CultureInfo.InvariantCulture, $"{L("total")}: {Money(order.Total)}");
        if (order.TrackingNumber is not null)
        {
            text.AppendLine(
                CultureInfo.InvariantCulture,
                $"{L("tracking")}: {order.TrackingNumber}"
            );
        }

        if (status == OrderStatuses.Invoiced)
        {
            text.AppendLine(CultureInfo.InvariantCulture, $"{L("invoice")}: {order.InvoiceNumber}");
        }

        text.AppendLine()
            .AppendLine(
                CultureInfo.InvariantCulture,
                $"{L("button")}: {links.Order(order.OrderId)}"
            )
            .AppendLine()
            .AppendLine(L("test"));

        return new EmailMessage(
            order.CustomerEmail,
            order.CustomerName,
            Fill(texts.Subject),
            html.ToString(),
            text.ToString(),
            $"order-{order.OrderId:N}-{status}@orderflow.local"
        );
    }

    private static string ProgressBar(string status, Func<string, string> label)
    {
        var current = Array.IndexOf(Progress, status);
        var cells = Progress.Select(
            (step, index) =>
            {
                var done = index <= current;
                var color = done ? "#2563eb" : "#cbd5e1";
                var text = done ? "#0f172a" : "#94a3b8";
                return $"""
                    <td align="center" width="20%" style="padding:0 2px;">
                        <div style="height:6px;border-radius:3px;background-color:{color};"></div>
                        <p style="margin:6px 0 0 0;font-size:11px;line-height:14px;color:{text};font-weight:{(
                        index == current ? 700 : 400
                    )};">{WebUtility.HtmlEncode(label($"step.{step}"))}</p>
                    </td>
                    """;
            }
        );
        return $"""
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="margin:0 0 24px 0;"><tr>{string.Concat(
                cells
            )}</tr></table>
            """;
    }

    private static string Highlight(OrderStatusChanged message, Func<string, string> label)
    {
        var order = message.Order;
        var rows = new List<(string Label, string Value)>();
        if (message.Status == OrderStatuses.Shipped && order.TrackingNumber is not null)
        {
            rows.Add((label("tracking"), order.TrackingNumber));
        }

        if (message.Status == OrderStatuses.Invoiced && order.InvoiceNumber is not null)
        {
            rows.Add((label("invoice"), order.InvoiceNumber));
            rows.Add(
                (label("invoiceDate"), EmailFormat.DateTime(message.OccurredAt, order.Locale))
            );
        }

        if (rows.Count == 0)
        {
            return string.Empty;
        }

        var cells = string.Concat(
            rows.Select(row =>
                $"""
                    <tr>
                        <td style="padding:6px 0;color:#64748b;">{WebUtility.HtmlEncode(
                        row.Label
                    )}</td>
                        <td align="right" style="padding:6px 0;color:#0f172a;font-weight:700;font-family:SFMono-Regular,Menlo,Consolas,monospace;">{WebUtility.HtmlEncode(
                        row.Value
                    )}</td>
                    </tr>
                    """
            )
        );
        return $"""
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="margin:0 0 24px 0;background-color:#eff6ff;border:1px solid #dbeafe;border-radius:12px;"><tr><td style="padding:12px 16px;"><table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="font-size:14px;line-height:20px;">{cells}</table></td></tr></table>
            """;
    }

    private static string TotalRows(
        OrderSnapshot order,
        string status,
        Func<string, string> label,
        Func<decimal, string> money,
        Func<string?, string> encode
    )
    {
        var rows = new List<(string Label, string Value, bool Strong)>
        {
            (label("subtotal"), money(order.Subtotal), false),
            (
                label("shipping"),
                order.ShippingFee == 0 ? label("free") : money(order.ShippingFee),
                false
            ),
        };
        if (status == OrderStatuses.Invoiced)
        {
            var net = decimal.Round(order.Total / (1 + VatRate), 2);
            rows.Add((label("net"), money(net), false));
            rows.Add((label("vat"), money(order.Total - net), false));
        }

        rows.Add((label("total"), money(order.Total), true));
        return string.Concat(
            rows.Select(row =>
                $"""
                    <tr>
                        <td style="padding:4px 0;color:{(
                        row.Strong ? "#0f172a" : "#64748b"
                    )};font-weight:{(row.Strong ? 700 : 400)};{(
                        row.Strong ? "font-size:16px;padding-top:10px;" : string.Empty
                    )}">{encode(row.Label)}</td>
                        <td align="right" style="padding:4px 0;color:#0f172a;font-weight:{(
                        row.Strong ? 700 : 600
                    )};{(row.Strong ? "font-size:16px;padding-top:10px;" : string.Empty)}">{encode(
                        row.Value
                    )}</td>
                    </tr>
                    """
            )
        );
    }

    private static string LoadTemplate()
    {
        using var stream =
            typeof(OrderEmail).Assembly.GetManifestResourceStream(TemplateName)
            ?? throw new InvalidOperationException($"Email template '{TemplateName}' is missing.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
