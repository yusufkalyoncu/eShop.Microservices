using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Basket.API.Features.RemoveItemFromBasket;

public record RemoveItemFromBasketResponse(bool IsSuccess);

public sealed class RemoveItemFromBasketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/basket/{userName}/items/{productId}", async (string userName, Guid productId, ICommandHandler<RemoveItemFromBasketCommand, RemoveItemFromBasketResult> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new RemoveItemFromBasketCommand(userName, productId), ct);

            return result.Match(
                success => new RemoveItemFromBasketResponse(success.IsSuccess));
        })
        .WithName("RemoveItemFromBasket")
        .Produces<RemoveItemFromBasketResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Remove Item From Basket")
        .WithDescription("Remove a single item from the basket");
    }
}