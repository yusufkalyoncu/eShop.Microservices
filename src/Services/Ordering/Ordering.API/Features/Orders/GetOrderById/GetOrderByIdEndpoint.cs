using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Security;

namespace Ordering.API.Features.Orders.GetOrderById;

public class GetOrderByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/id/{id}", async (Guid id, ICurrentUser currentUser, IQueryHandler<GetOrderByIdQuery, GetOrderByIdResult> handler, CancellationToken ct) =>
        {
            var userName = currentUser.GetRequiredName();
            var result = await handler.Handle(new GetOrderByIdQuery(id, userName), ct);
            return result.Match(res => res);
        })
        .WithName("GetOrderById")
        .Produces<GetOrderByIdResult>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Order By Id")
        .WithDescription("Get order details by its Id")
        .RequireAuthorization();
    }
}