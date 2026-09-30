using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Inventory.API.Features.Stock.ReserveStock;

public record ReserveStockRequest(Dictionary<Guid, int> Items);

public sealed class ReserveStockEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/stock/reserve", async (ReserveStockRequest request, ICommandHandler<ReserveStockCommand> handler, CancellationToken ct) =>
        {
            var command = new ReserveStockCommand(request.Items);
            var result = await handler.Handle(command, ct);
            return result.Match();
        })
        .WithTags("Stock")
        .WithSummary("Reserve stock for a product")
        .WithDescription("Reserves stock for a product, throwing an error if insufficient.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}