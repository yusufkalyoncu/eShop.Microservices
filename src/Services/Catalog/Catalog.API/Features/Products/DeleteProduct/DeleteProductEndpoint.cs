using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Security;

namespace Catalog.API.Features.Products.DeleteProduct;

public sealed class DeleteProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{id:guid}", async (Guid id, ICommandHandler<DeleteProductCommand> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new DeleteProductCommand(id), ct);
            return result.Match();
        })
        .WithTags("Products")
        .WithSummary("Deletes a product")
        .WithDescription("Deletes a product by its ID")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.Admin);
    }
}