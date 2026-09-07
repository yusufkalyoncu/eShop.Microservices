using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Pagination;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Features.Products.GetProducts;

public sealed class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async ([FromQuery] int pageIndex, [FromQuery] int pageSize, IQueryHandler<GetProductsQuery, PaginatedResult<ProductDto>> handler, CancellationToken ct) =>
        {
            var query = new GetProductsQuery(pageIndex > 0 ? pageIndex : 1, pageSize > 0 ? pageSize : 10);
            var result = await handler.Handle(query, ct);
            return result.Match();
        })
        .WithTags("Products")
        .WithSummary("Gets all products")
        .WithDescription("Retrieves a paginated list of all products")
        .Produces<PaginatedResult<ProductDto>>();
    }
}