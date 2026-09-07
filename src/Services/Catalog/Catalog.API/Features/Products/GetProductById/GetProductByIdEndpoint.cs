using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Catalog.API.Features.Products.GetProductById;

public sealed class GetProductByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id:guid}", async (Guid id, IQueryHandler<GetProductByIdQuery, ProductDetailsDto> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetProductByIdQuery(id), ct);
            return result.Match();
        })
        .WithTags("Products")
        .WithSummary("Gets a product by ID")
        .WithDescription("Retrieves a product's details by its ID")
        .Produces<ProductDetailsDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}