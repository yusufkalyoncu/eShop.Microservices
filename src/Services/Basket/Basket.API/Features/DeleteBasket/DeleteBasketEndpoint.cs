using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Basket.API.Features.DeleteBasket;

public record DeleteBasketResponse(bool IsSuccess);

public sealed class DeleteBasketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/basket/{userName}", async (string userName, ICommandHandler<DeleteBasketCommand, DeleteBasketResult> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new DeleteBasketCommand(userName), ct);
            
            return result.Match(success => new DeleteBasketResponse(success.IsSuccess));
        })
        .WithName("DeleteBasket")
        .Produces<DeleteBasketResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Delete Basket")
        .WithDescription("Delete Basket");
    }
}