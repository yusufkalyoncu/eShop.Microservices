using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Catalog.API.Features.Categories.GetCategories;

public sealed class GetCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", async (IQueryHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetCategoriesQuery(), ct);
            return result.Match();
        })
        .WithTags("Categories")
        .WithSummary("Gets all categories")
        .WithDescription("Retrieves a list of all categories")
        .Produces<IReadOnlyList<CategoryDto>>();
    }
}