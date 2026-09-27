using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Ordering.API.Features.Orders.GetOrders;

public class GetOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders", async (BuildingBlocks.Web.Security.ICurrentUser currentUser, IQueryHandler<GetOrdersQuery, GetOrdersResult> handler, CancellationToken ct) =>
        {
            var userName = currentUser.Name ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var result = await handler.Handle(new GetOrdersQuery(userName), ct);
            return result.Match(res => res);
        })
        .WithName("GetOrders")
        .Produces<GetOrdersResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Orders")
        .WithDescription("Get a list of orders for a user")
        .RequireAuthorization();
    }
}