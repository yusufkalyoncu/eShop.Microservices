using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Security;

namespace Catalog.API.Features.Products.UpdateProduct;

public sealed class UpdateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{id:guid}", async (Guid id, UpdateProductCommand command, ICommandHandler<UpdateProductCommand> handler, CancellationToken ct) =>
        {
            if (id != command.Id)
            {
                return Results.BadRequest("Id in route does not match id in body.");
            }

            var result = await handler.Handle(command, ct);
            return result.Match();
        })
        .WithTags("Products")
        .WithSummary("Updates a product")
        .WithDescription("Updates an existing product by its ID")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Admin);
    }
}