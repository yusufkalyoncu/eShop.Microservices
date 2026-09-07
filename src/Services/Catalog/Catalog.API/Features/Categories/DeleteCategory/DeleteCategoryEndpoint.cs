using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Catalog.API.Features.Categories.DeleteCategory;

public sealed class DeleteCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/categories/{id:guid}", async (Guid id, ICommandHandler<DeleteCategoryCommand> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new DeleteCategoryCommand(id), ct);
            return result.Match();
        })
        .WithTags("Categories")
        .WithSummary("Deletes a category")
        .WithDescription("Deletes a category by its ID")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}