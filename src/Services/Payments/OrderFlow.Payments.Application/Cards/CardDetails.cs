namespace OrderFlow.Payments.Application.Cards;

public sealed record CardDetails(
    string HolderName,
    string Number,
    int ExpiryMonth,
    int ExpiryYear,
    string Cvv
)
{
    public string Last4 => Number[^4..];

    public string Brand => CardBrands.Detect(Number);

    public override string ToString() => $"CardDetails {{ Brand = {Brand}, Last4 = {Last4} }}";
}
