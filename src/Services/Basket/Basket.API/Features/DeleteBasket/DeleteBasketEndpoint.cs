using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Security;

namespace Basket.API.Features.DeleteBasket;

public record DeleteBasketResponse(bool IsSuccess);

public sealed class DeleteBasketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/basket", async (ICurrentUser currentUser, ICommandHandler<DeleteBasketCommand, DeleteBasketResult> handler, CancellationToken ct) =>
        {
            var userName = currentUser.GetRequiredName();
            var result = await handler.Handle(new DeleteBasketCommand(userName), ct);

            return result.Match(success => Results.Ok(new DeleteBasketResponse(success.IsSuccess)));
        })
        .WithName("DeleteBasket")
        .Produces<DeleteBasketResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Delete Basket for Current User")
        .WithDescription("Delete Basket for Current User")
        .RequireAuthorization();
    }
}