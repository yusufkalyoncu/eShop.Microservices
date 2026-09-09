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
        app.MapPut("/basket/{userName}/items/{productId}", async (string userName, Guid productId, UpdateItemQuantityInBasketRequest request, ICommandHandler<UpdateItemQuantityInBasketCommand, UpdateItemQuantityInBasketResult> handler, CancellationToken ct) =>
        {
            var command = new UpdateItemQuantityInBasketCommand(userName, productId, request.Quantity);
            var result = await handler.Handle(command, ct);

            return result.Match(
                success => new UpdateItemQuantityInBasketResponse(success.IsSuccess));
        })
        .WithName("UpdateItemQuantityInBasket")
        .Produces<UpdateItemQuantityInBasketResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Update Item Quantity In Basket")
        .WithDescription("Update quantity of a single item in the basket. Set quantity to 0 to remove the item. Maximum quantity per item is 10.");
    }
}
