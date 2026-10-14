namespace OrderFlow.Notifications.Application.Deposits;

internal sealed record WalletEmailTexts(
    string Subject,
    string Preheader,
    string Heading,
    string Greeting,
    string GreetingNoName,
    string Intro,
    string AmountLabel,
    string NewBalance,
    string Card,
    string Date,
    string CodeLabel,
    string Reference,
    string Button,
    string TestNotice,
    string Footer
)
{
    private const string FooterEn =
        "You received this email because a payment was made on your OrderFlow account. If this wasn't you, contact support@orderflow.local.";
    private const string FooterTr =
        "Bu e-postayı OrderFlow hesabında bir ödeme yapıldığı için aldın. Bu işlemi sen yapmadıysan support@orderflow.local adresine yaz.";

    private static readonly WalletEmailTexts DepositEn = new(
        Subject: "Your OrderFlow balance was topped up with {amount}",
        Preheader: "{amount} was added to your wallet. New balance: {balance}.",
        Heading: "Your balance was topped up",
        Greeting: "Hi {name},",
        GreetingNoName: "Hi,",
        Intro: "We received your payment and added it to your OrderFlow wallet. You can use your balance at checkout right away.",
        AmountLabel: "Amount added",
        NewBalance: "New balance",
        Card: "Card",
        Date: "Date",
        CodeLabel: "Authorization code",
        Reference: "Reference",
        Button: "View my wallet",
        TestNotice: "This was a test payment. No money was charged to your card.",
        Footer: FooterEn
    );

    private static readonly WalletEmailTexts DepositTr = new(
        Subject: "OrderFlow bakiyene {amount} yüklendi",
        Preheader: "Cüzdanına {amount} eklendi. Yeni bakiye: {balance}.",
        Heading: "Bakiyen yüklendi",
        Greeting: "Merhaba {name},",
        GreetingNoName: "Merhaba,",
        Intro: "Ödemeni aldık ve OrderFlow cüzdanına ekledik. Bakiyeni hemen alışverişlerinde kullanabilirsin.",
        AmountLabel: "Yüklenen tutar",
        NewBalance: "Yeni bakiye",
        Card: "Kart",
        Date: "Tarih",
        CodeLabel: "Provizyon kodu",
        Reference: "Referans",
        Button: "Cüzdanımı görüntüle",
        TestNotice: "Bu bir test ödemesiydi. Kartından para çekilmedi.",
        Footer: FooterTr
    );

    private static readonly WalletEmailTexts ReversalEn = new(
        Subject: "Your OrderFlow top-up of {amount} was reversed",
        Preheader: "{amount} was deducted from your wallet. New balance: {balance}.",
        Heading: "Your top-up was reversed",
        Greeting: "Hi {name},",
        GreetingNoName: "Hi,",
        Intro: "Your card issuer declined the final settlement of this top-up. We deducted the amount from your wallet and released the authorization on your card, so you will not be charged.",
        AmountLabel: "Amount deducted",
        NewBalance: "New balance",
        Card: "Card",
        Date: "Date",
        CodeLabel: "Reason",
        Reference: "Reference",
        Button: "View my wallet",
        TestNotice: "This was a test payment with a card that is set up to fail at settlement. No money was charged.",
        Footer: FooterEn
    );

    private static readonly WalletEmailTexts ReversalTr = new(
        Subject: "OrderFlow'daki {amount} tutarındaki yüklemen iptal edildi",
        Preheader: "Cüzdanından {amount} düşüldü. Yeni bakiye: {balance}.",
        Heading: "Bakiye yüklemen geri alındı",
        Greeting: "Merhaba {name},",
        GreetingNoName: "Merhaba,",
        Intro: "Kartını veren banka bu yüklemenin takasını (settlement) onaylamadı. Tutarı cüzdanından düştük ve kartındaki provizyonu iptal ettik; kartından para çekilmeyecek.",
        AmountLabel: "Düşülen tutar",
        NewBalance: "Yeni bakiye",
        Card: "Kart",
        Date: "Tarih",
        CodeLabel: "Neden",
        Reference: "Referans",
        Button: "Cüzdanımı görüntüle",
        TestNotice: "Bu, takas aşamasında hata verecek şekilde ayarlanmış bir test kartıyla yapılan test ödemesiydi. Kartından para çekilmedi.",
        Footer: FooterTr
    );

    private static readonly Dictionary<string, (string En, string Tr)> Reasons = new(
        StringComparer.Ordinal
    )
    {
        ["issuer_declined_settlement"] = (
            "Card issuer declined the settlement",
            "Banka takası reddetti"
        ),
    };

    public static WalletEmailTexts For(WalletEmailKind kind, string locale) =>
        (kind, locale) switch
        {
            (WalletEmailKind.Reversal, "tr") => ReversalTr,
            (WalletEmailKind.Reversal, _) => ReversalEn,
            (_, "tr") => DepositTr,
            _ => DepositEn,
        };

    public static string Reason(string code, string locale)
    {
        if (!Reasons.TryGetValue(code, out var text))
        {
            return code;
        }

        return locale == "tr" ? text.Tr : text.En;
    }
}
