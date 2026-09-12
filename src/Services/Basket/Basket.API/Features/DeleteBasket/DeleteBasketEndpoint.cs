using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Basket.API.Features.DeleteBasket;

public record DeleteBasketResponse(bool IsSuccess);

public sealed class DeleteBasketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/basket", async (BuildingBlocks.Web.Security.ICurrentUser currentUser, ICommandHandler<DeleteBasketCommand, DeleteBasketResult> handler, CancellationToken ct) =>
        {
            var userName = currentUser.Name ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var result = await handler.Handle(new DeleteBasketCommand(userName), ct);

            return result.Match(success => Results.Ok(new DeleteBasketResponse(success.IsSuccess)));
        })
        .WithName("DeleteBasket")
        .Produces<DeleteBasketResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Delete Basket for Current User")
        .WithDescription("Delete Basket for Current User")
        .RequireAuthorization();
    }
}