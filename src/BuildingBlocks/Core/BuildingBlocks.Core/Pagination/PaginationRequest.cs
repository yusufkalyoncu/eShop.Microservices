namespace BuildingBlocks.Core.Pagination;

public record PaginationRequest
{
    private const int MaxPageSize = 100;

    public int PageIndex
    {
        get;
        init => field = value < 1 ? 1 : value;
    } = 1;

    public int PageSize
    {
        get;
        init => field = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    } = 10;
}