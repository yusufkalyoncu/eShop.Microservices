using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Basket.API.Features.GetBasket;

public record GetBasketResponse(ShoppingCartDto Cart);
public record ShoppingCartDto(string UserName, List<ShoppingCartItemDto> Items, decimal TotalPrice);
public record ShoppingCartItemDto(Guid ProductId, string ProductName, int Quantity, decimal Price);

public sealed class GetBasketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/basket/{userName}", async (string userName, IQueryHandler<GetBasketQuery, GetBasketResult> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetBasketQuery(userName), ct);

            return result.Match(success =>
            {
                var dto = new ShoppingCartDto(
                    success.Cart.UserName,
                    success.Cart.Items.Select(x => new ShoppingCartItemDto(x.ProductId, x.ProductName, x.Quantity, x.Price)).ToList(),
                    success.Cart.TotalPrice);
                    
                return new GetBasketResponse(dto);
            });
        })
        .WithName("GetBasket")
        .Produces<GetBasketResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Basket By Username")
        .WithDescription("Get Basket By Username");
    }
}