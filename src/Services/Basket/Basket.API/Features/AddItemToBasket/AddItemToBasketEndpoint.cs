using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Basket.API.Features.AddItemToBasket;

public record AddItemToBasketRequest(string UserName, Guid ProductId, int Quantity);
public record AddItemToBasketResponse(string UserName);

public class AddItemToBasketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket/items", async (AddItemToBasketRequest request, ICommandHandler<AddItemToBasketCommand, AddItemToBasketResult> handler, CancellationToken ct) =>
        {
            var command = new AddItemToBasketCommand(request.UserName, request.ProductId, request.Quantity);
            var result = await handler.Handle(command, ct);
            
            return result.Match(success => Results.Ok(new AddItemToBasketResponse(success.UserName)));
        })
        .WithName("AddItemToBasket")
        .Produces<AddItemToBasketResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Add Item To Basket")
        .WithDescription("Adds a product to the user's shopping basket by querying its info from the Catalog.");
    }
}