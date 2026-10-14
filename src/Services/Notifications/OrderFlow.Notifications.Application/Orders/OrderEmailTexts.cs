using OrderFlow.Orders.Contracts.Orders;

namespace OrderFlow.Notifications.Application.Orders;

internal sealed record OrderEmailTexts(string Subject, string Heading, string Intro);

internal static class OrderEmailCopy
{
    private static readonly Dictionary<(string Status, string Locale), OrderEmailTexts> Texts =
        new()
        {
            [(OrderStatuses.Placed, "en")] = new(
                "We received your order {number}",
                "Thanks for your order!",
                "We received your order and are now reserving the items and taking the payment. You will get another email as soon as it is confirmed."
            ),
            [(OrderStatuses.Placed, "tr")] = new(
                "{number} numaralı siparişini aldık",
                "Siparişin için teşekkürler!",
                "Siparişini aldık; ürünleri ayırıyor ve ödemeyi alıyoruz. Sipariş onaylanınca sana tekrar e-posta göndereceğiz."
            ),
            [(OrderStatuses.Confirmed, "en")] = new(
                "Your order {number} is confirmed",
                "Your order is confirmed",
                "The items are reserved for you and the payment was successful. We are preparing your package."
            ),
            [(OrderStatuses.Confirmed, "tr")] = new(
                "{number} numaralı siparişin onaylandı",
                "Siparişin onaylandı",
                "Ürünler senin için ayrıldı ve ödeme başarıyla alındı. Paketini hazırlıyoruz."
            ),
            [(OrderStatuses.Shipped, "en")] = new(
                "Your order {number} is on its way",
                "Your order has shipped",
                "Your package left our warehouse and is on its way to you. You can follow it with the tracking number below."
            ),
            [(OrderStatuses.Shipped, "tr")] = new(
                "{number} numaralı siparişin kargoya verildi",
                "Siparişin kargoda",
                "Paketin depomuzdan çıktı ve sana doğru yola çıktı. Aşağıdaki takip numarasıyla izleyebilirsin."
            ),
            [(OrderStatuses.Delivered, "en")] = new(
                "Your order {number} was delivered",
                "Your order was delivered",
                "Your package was delivered. We hope you enjoy it! Your invoice will follow in a separate email."
            ),
            [(OrderStatuses.Delivered, "tr")] = new(
                "{number} numaralı siparişin teslim edildi",
                "Siparişin teslim edildi",
                "Paketin teslim edildi. Keyifli kullanımlar! Faturan ayrı bir e-postayla gelecek."
            ),
            [(OrderStatuses.Invoiced, "en")] = new(
                "Invoice {invoice} for your order {number}",
                "Your invoice",
                "Here is the invoice for your order. Prices include 20% VAT."
            ),
            [(OrderStatuses.Invoiced, "tr")] = new(
                "{number} numaralı siparişinin faturası ({invoice})",
                "Faturan",
                "Siparişinin faturası aşağıda. Fiyatlara %20 KDV dahildir."
            ),
            [(OrderStatuses.Cancelled, "en")] = new(
                "Your order {number} was cancelled",
                "Your order was cancelled",
                "Unfortunately we could not complete your order: {reason}. Nothing was charged; any reserved items and card authorization were released."
            ),
            [(OrderStatuses.Cancelled, "tr")] = new(
                "{number} numaralı siparişin iptal edildi",
                "Siparişin iptal edildi",
                "Maalesef siparişini tamamlayamadık: {reason}. Hiçbir ücret alınmadı; ayrılan ürünler ve kart provizyonu serbest bırakıldı."
            ),
            [(OrderStatuses.Refunded, "en")] = new(
                "Your order {number} was refunded",
                "Your money is on its way back",
                "The carrier lost your package, so we cancelled the delivery and refunded the full amount to your original payment method."
            ),
            [(OrderStatuses.Refunded, "tr")] = new(
                "{number} numaralı siparişin için iade yapıldı",
                "Paran iade edildi",
                "Kargo firması paketini kaybetti; teslimatı iptal ettik ve tutarın tamamını ödeme yaptığın yönteme iade ettik."
            ),
        };

    private static readonly Dictionary<string, (string En, string Tr)> Reasons = new(
        StringComparer.Ordinal
    )
    {
        ["out_of_stock"] = ("some items are out of stock", "bazı ürünler stokta yok"),
        ["payments.insufficient_funds"] = (
            "your wallet balance is not enough",
            "cüzdan bakiyen yetersiz"
        ),
        ["card_capture_declined"] = (
            "your card issuer declined the payment",
            "bankan ödemeyi reddetti"
        ),
        ["payments.card_payment_amount_mismatch"] = (
            "the card authorization does not match the order total",
            "kart provizyonu sipariş tutarıyla eşleşmiyor"
        ),
        ["payments.card_payment_not_found"] = (
            "the card authorization was not found",
            "kart provizyonu bulunamadı"
        ),
        ["payments.card_payment_not_authorized"] = (
            "the card authorization is no longer valid",
            "kart provizyonu artık geçerli değil"
        ),
    };

    public static OrderEmailTexts For(string status, string locale) =>
        Texts.TryGetValue((status, locale), out var texts) ? texts : Texts[(status, "en")];

    public static string Reason(string? code, string locale)
    {
        return code is null || !Reasons.TryGetValue(code, out var reason)
            ? locale == "tr" ? "beklenmeyen bir hata oluştu" : "an unexpected error occurred"
            : locale == "tr" ? reason.Tr : reason.En;
    }

    public static string Label(string key, string locale) =>
        (key, locale) switch
        {
            ("order", "tr") => "Sipariş",
            ("order", _) => "Order",
            ("items", "tr") => "Ürünler",
            ("items", _) => "Items",
            ("subtotal", "tr") => "Ara toplam",
            ("subtotal", _) => "Subtotal",
            ("shipping", "tr") => "Kargo",
            ("shipping", _) => "Shipping",
            ("free", "tr") => "Ücretsiz",
            ("free", _) => "Free",
            ("total", "tr") => "Toplam",
            ("total", _) => "Total",
            ("net", "tr") => "KDV hariç tutar",
            ("net", _) => "Net amount",
            ("vat", "tr") => "KDV (%20)",
            ("vat", _) => "VAT (20%)",
            ("address", "tr") => "Teslimat adresi",
            ("address", _) => "Shipping address",
            ("payment", "tr") => "Ödeme",
            ("payment", _) => "Payment",
            ("wallet", "tr") => "OrderFlow cüzdanı",
            ("wallet", _) => "OrderFlow wallet",
            ("button", "tr") => "Siparişi görüntüle",
            ("button", _) => "View order",
            ("tracking", "tr") => "Kargo takip numarası",
            ("tracking", _) => "Tracking number",
            ("invoice", "tr") => "Fatura numarası",
            ("invoice", _) => "Invoice number",
            ("invoiceDate", "tr") => "Fatura tarihi",
            ("invoiceDate", _) => "Invoice date",
            ("billedTo", "tr") => "Fatura adresi",
            ("billedTo", _) => "Billed to",
            ("greeting", "tr") => "Merhaba {name},",
            ("greeting", _) => "Hi {name},",
            ("greetingNoName", "tr") => "Merhaba,",
            ("greetingNoName", _) => "Hi,",
            ("test", "tr") =>
                "Bu bir test siparişidir: ödeme, kargo ve fatura simülasyondur, gerçek bir işlem yapılmadı.",
            ("test", _) =>
                "This is a test order: payment, shipping, and invoice are simulated; nothing real happened.",
            ("footer", "tr") =>
                "Bu e-postayı OrderFlow hesabında verdiğin bir sipariş için aldın. Sorun olursa support@orderflow.local adresine yaz.",
            ("footer", _) =>
                "You received this email about an order on your OrderFlow account. Questions? Write to support@orderflow.local.",
            ("step.placed", "tr") => "Alındı",
            ("step.placed", _) => "Placed",
            ("step.confirmed", "tr") => "Onaylandı",
            ("step.confirmed", _) => "Confirmed",
            ("step.shipped", "tr") => "Kargoda",
            ("step.shipped", _) => "Shipped",
            ("step.delivered", "tr") => "Teslim edildi",
            ("step.delivered", _) => "Delivered",
            ("step.invoiced", "tr") => "Faturalandı",
            ("step.invoiced", _) => "Invoiced",
            _ => key,
        };
}
