using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Security;

namespace Basket.API.Features.AddItemToBasket;

public record AddItemToBasketRequest(Guid ProductId, int Quantity);
public record AddItemToBasketResponse(string UserName);

public class AddItemToBasketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket", async (AddItemToBasketRequest request, ICurrentUser currentUser, ICommandHandler<AddItemToBasketCommand, AddItemToBasketResult> handler, CancellationToken ct) =>
        {
            var userName = currentUser.GetRequiredName();
            var command = new AddItemToBasketCommand(userName, request.ProductId, request.Quantity);

            var result = await handler.Handle(command, ct);

            return result.Match(success => Results.Ok(new AddItemToBasketResponse(success.UserName)));
        })
        .WithName("AddItemToBasket")
        .Produces<AddItemToBasketResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Add Item To Basket for Current User")
        .WithDescription("Add Item To Basket for Current User")
        .RequireAuthorization();
    }
}