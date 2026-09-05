using BuildingBlocks.Core.Pagination;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Persistence.EntityFrameworkCore.Pagination;

public static class QueryableExtensions
{
    public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
        this IQueryable<T> query,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var count = await query.CountAsync(cancellationToken);

        var data = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>(pageIndex, pageSize, count, data);
    }
}