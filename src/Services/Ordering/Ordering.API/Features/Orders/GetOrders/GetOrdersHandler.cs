using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Infrastructure.Database;

namespace Ordering.API.Features.Orders.GetOrders;

public class GetOrdersQueryHandler(OrderingDbContext dbContext) : IQueryHandler<GetOrdersQuery, GetOrdersResult>
{
    public async Task<Result<GetOrdersResult>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.UserName == request.UserName)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderDto(o.Id, o.UserName, o.TotalPrice, o.Status.ToString(), o.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetOrdersResult(orders));
    }
}