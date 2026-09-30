using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Inventory.API.Features.Stock.ReleaseStock;

public record ReleaseStockRequest(Dictionary<Guid, int> Items);

public sealed class ReleaseStockEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/stock/release", async (ReleaseStockRequest request, ICommandHandler<ReleaseStockCommand> handler, CancellationToken ct) =>
        {
            var command = new ReleaseStockCommand(request.Items);
            var result = await handler.Handle(command, ct);
            return result.Match();
        })
        .WithTags("Stock")
        .WithSummary("Release reserved stock for a product")
        .WithDescription("Releases previously reserved stock for a product back into available inventory.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}