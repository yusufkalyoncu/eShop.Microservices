using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Basket.API.Features.UpdateItemQuantityInBasket;

public record UpdateItemQuantityInBasketRequest(int Quantity);
public record UpdateItemQuantityInBasketResponse(bool IsSuccess);

public sealed class UpdateItemQuantityInBasketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/basket/items/{productId}", async (Guid productId, UpdateItemQuantityInBasketRequest request, BuildingBlocks.Web.Security.ICurrentUser currentUser, ICommandHandler<UpdateItemQuantityInBasketCommand, UpdateItemQuantityInBasketResult> handler, CancellationToken ct) =>
        {
            var userName = currentUser.Name ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var command = new UpdateItemQuantityInBasketCommand(userName, productId, request.Quantity);
            var result = await handler.Handle(command, ct);

            return result.Match(success => Results.Ok(new UpdateItemQuantityInBasketResponse(success.IsSuccess)));
        })
        .WithName("UpdateItemQuantityInBasket")
        .Produces<UpdateItemQuantityInBasketResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Update Item Quantity In Basket for Current User")
        .WithDescription("Update quantity of a single item in the basket for the current user. Set quantity to 0 to remove the item. Maximum quantity per item is 10.")
        .RequireAuthorization();
    }
}
