namespace OrderFlow.Products.Application.Products.GetProductsByIds;

public sealed record GetProductsByIdsQuery(IReadOnlyList<Guid> Ids)
{
    public const int MaxIds = 100;
}
