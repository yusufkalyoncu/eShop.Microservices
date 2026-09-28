using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Inventory.API.Features.Stock.AddStock;

public record AddStockRequest(Guid ProductId, int Quantity);

public sealed class AddStockEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/stock/add", async (AddStockRequest request, ICommandHandler<AddStockCommand> handler, CancellationToken ct) =>
        {
            var command = new AddStockCommand(request.ProductId, request.Quantity);
            var result = await handler.Handle(command, ct);
            return result.Match();
        })
        .WithTags("Stock")
        .WithSummary("Add stock for a product")
        .WithDescription("Increases the available stock for a product")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}