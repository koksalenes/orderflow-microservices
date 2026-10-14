namespace OrderFlow.BuildingBlocks.Application.Paging;

public static class PageRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static int Offset(int page, int pageSize) => (page - 1) * pageSize;
}
