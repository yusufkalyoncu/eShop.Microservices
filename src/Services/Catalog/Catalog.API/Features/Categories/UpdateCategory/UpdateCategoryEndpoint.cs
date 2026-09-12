using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Security;

namespace Catalog.API.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/categories/{id:guid}", async (Guid id, UpdateCategoryCommand command, ICommandHandler<UpdateCategoryCommand> handler, CancellationToken ct) =>
        {
            if (id != command.Id)
            {
                return Results.BadRequest("Id in route does not match id in body.");
            }

            var result = await handler.Handle(command, ct);
            return result.Match();
        })
        .WithTags("Categories")
        .WithSummary("Updates a category")
        .WithDescription("Updates an existing category by its ID")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Admin);
    }
}