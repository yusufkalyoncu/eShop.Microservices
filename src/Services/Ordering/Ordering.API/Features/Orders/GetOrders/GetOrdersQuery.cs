using BuildingBlocks.Core.CQRS;

namespace Ordering.API.Features.Orders.GetOrders;

public record GetOrdersQuery(string UserName) : IQuery<GetOrdersResult>;

public record GetOrdersResult(List<OrderDto> Orders);

public record OrderDto(Guid Id, string UserName, decimal TotalPrice, string Status, DateTime CreatedAt);