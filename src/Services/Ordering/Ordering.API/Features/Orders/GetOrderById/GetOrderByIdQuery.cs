using BuildingBlocks.Core.CQRS;

namespace Ordering.API.Features.Orders.GetOrderById;

public record GetOrderByIdQuery(Guid Id, string UserName) : IQuery<GetOrderByIdResult>;

public record GetOrderByIdResult(OrderDetailsDto Order);

public record OrderDetailsDto(Guid Id, string UserName, decimal TotalPrice, string Status, DateTime CreatedAt, List<OrderItemDto> Items);

public record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal Price);