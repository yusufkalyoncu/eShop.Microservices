using Basket.API.Domain.Models;
using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Basket.API.Features.StoreBasket;

public record StoreBasketRequest(ShoppingCartDto Cart);
public record StoreBasketResponse(string UserName);

public record ShoppingCartDto(string UserName, List<ShoppingCartItemDto> Items);
public record ShoppingCartItemDto(Guid ProductId, string ProductName, int Quantity, decimal Price);

public sealed class StoreBasketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket", async (StoreBasketRequest request, ICommandHandler<StoreBasketCommand, StoreBasketResult> handler, CancellationToken ct) =>
        {
            var cart = new ShoppingCart(request.Cart.UserName);
            foreach (var item in request.Cart.Items)
            {
                cart.Items.Add(new ShoppingCartItem(item.ProductId, item.ProductName, item.Quantity, item.Price));
            }

            var command = new StoreBasketCommand(cart);
            var result = await handler.Handle(command, ct);

            return result.Match(success => new StoreBasketResponse(success.UserName));
        })
        .WithName("StoreBasket")
        .Produces<StoreBasketResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Store Basket")
        .WithDescription("Store Basket");
    }
}