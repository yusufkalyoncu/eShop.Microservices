namespace BuildingBlocks.Core.Pagination;

public sealed record PaginatedResult<T>(
    int PageIndex,
    int PageSize,
    long Count,
    IReadOnlyList<T> Data)
{
    public int TotalPages => Count == 0
        ? 0
        : (int)Math.Ceiling(Count / (double)PageSize);

    public bool HasPreviousPage => PageIndex > 1;

    public bool HasNextPage => PageIndex < TotalPages;
}