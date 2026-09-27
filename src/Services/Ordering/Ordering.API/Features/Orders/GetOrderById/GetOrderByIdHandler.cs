using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Domain.Errors;
using Ordering.API.Infrastructure.Database;

namespace Ordering.API.Features.Orders.GetOrderById;

public class GetOrderByIdQueryHandler(OrderingDbContext dbContext) : IQueryHandler<GetOrderByIdQuery, GetOrderByIdResult>
{
    public async Task<Result<GetOrderByIdResult>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.UserName == request.UserName, cancellationToken);

        if (order == null)
        {
            return Result.Failure<GetOrderByIdResult>(OrderingErrors.Order.NotFound);
        }

        var orderDto = new OrderDetailsDto(
            order.Id,
            order.UserName,
            order.TotalPrice,
            order.Status.ToString(),
            order.CreatedAt,
            order.OrderItems.Select(oi => new OrderItemDto(oi.ProductId, oi.ProductName, oi.Quantity, oi.Price)).ToList()
        );

        return Result.Success(new GetOrderByIdResult(orderDto));
    }
}