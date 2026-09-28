using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Security;

namespace Basket.API.Features.RemoveItemFromBasket;

public record RemoveItemFromBasketResponse(bool IsSuccess);

public sealed class RemoveItemFromBasketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/basket/items/{productId}", async (Guid productId, ICurrentUser currentUser, ICommandHandler<RemoveItemFromBasketCommand, RemoveItemFromBasketResult> handler, CancellationToken ct) =>
        {
            var userName = currentUser.GetRequiredName();
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