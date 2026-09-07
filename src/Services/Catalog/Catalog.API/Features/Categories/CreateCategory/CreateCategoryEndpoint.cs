using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Catalog.API.Features.Categories.CreateCategory;

public sealed class CreateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/categories", async (CreateCategoryCommand command, ICommandHandler<CreateCategoryCommand, Guid> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.Match();
        })
        .WithTags("Categories")
        .WithSummary("Creates a new category")
        .WithDescription("Creates a new category for products")
        .Produces<Guid>()
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}