namespace OrderFlow.Payments.Application.Cards;

public static class CardBrands
{
    public const string Visa = "Visa";
    public const string Mastercard = "Mastercard";
    public const string Amex = "Amex";
    public const string Troy = "Troy";
    public const string Unknown = "Card";

    public static string Detect(string number)
    {
        ArgumentNullException.ThrowIfNull(number);

        if (number.StartsWith('4'))
        {
            return Visa;
        }

        return number.StartsWith("34", StringComparison.Ordinal)
            || number.StartsWith("37", StringComparison.Ordinal)
            ? Amex
            : number.StartsWith("9792", StringComparison.Ordinal)
            || number.StartsWith("65", StringComparison.Ordinal)
            ? Troy
            : number.Length >= 2 && number[0] is '5' or '2' ? Mastercard : Unknown;
    }

    public static int CvvLength(string number) => Detect(number) == Amex ? 4 : 3;
}
