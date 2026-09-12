using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Basket.API.Features.RemoveItemFromBasket;

public record RemoveItemFromBasketResponse(bool IsSuccess);

public sealed class RemoveItemFromBasketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/basket/items/{productId}", async (Guid productId, BuildingBlocks.Web.Security.ICurrentUser currentUser, ICommandHandler<RemoveItemFromBasketCommand, RemoveItemFromBasketResult> handler, CancellationToken ct) =>
        {
            var userName = currentUser.Name ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var result = await handler.Handle(new RemoveItemFromBasketCommand(userName, productId), ct);

            return result.Match(success => Results.Ok(new RemoveItemFromBasketResponse(success.IsSuccess)));
        })
        .WithName("RemoveItemFromBasket")
        .Produces<RemoveItemFromBasketResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Remove Item From Basket for Current User")
        .WithDescription("Remove Item From Basket for Current User")
        .RequireAuthorization();
    }
}