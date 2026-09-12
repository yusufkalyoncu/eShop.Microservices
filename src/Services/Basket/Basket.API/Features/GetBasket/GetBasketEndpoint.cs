using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Security;

namespace Basket.API.Features.GetBasket;

public record GetBasketResponse(ShoppingCartDto Cart);
public record ShoppingCartDto(string UserName, List<ShoppingCartItemDto> Items, decimal TotalPrice);
public record ShoppingCartItemDto(Guid ProductId, string ProductName, int Quantity, decimal Price);

public sealed class GetBasketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/basket", async (ICurrentUser currentUser, IQueryHandler<GetBasketQuery, GetBasketResult> handler, CancellationToken ct) =>
        {
            var userName = currentUser.Name ?? throw new UnauthorizedAccessException("User is not authenticated.");
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
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Get Basket for Current User")
        .WithDescription("Get Basket for Current User")
        .RequireAuthorization();
    }
}