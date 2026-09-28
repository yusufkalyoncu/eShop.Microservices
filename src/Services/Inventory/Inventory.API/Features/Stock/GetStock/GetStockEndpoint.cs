using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Inventory.API.Features.Stock.GetStock;

public record GetStockResponse(Guid ProductId, int AvailableQuantity);

public sealed class GetStockEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/stock/{productId:guid}", async (Guid productId, IQueryHandler<GetStockQuery, GetStockResult> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetStockQuery(productId), ct);
            return result.Match(res => new GetStockResponse(res.ProductId, res.AvailableQuantity));
        })
        .WithTags("Stock")
        .WithSummary("Get available stock for a product")
        .WithDescription("Gets available stock for a product")
        .Produces<GetStockResponse>();
    }
}