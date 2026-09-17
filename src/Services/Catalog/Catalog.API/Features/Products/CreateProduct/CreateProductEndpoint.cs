using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Security;

namespace Catalog.API.Features.Products.CreateProduct;

public record CreateProductResponse(Guid Id);

public sealed class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products",async (CreateProductCommand command, ICommandHandler<CreateProductCommand, Guid> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.Match(id => new CreateProductResponse(id));
        })
        .WithTags("Products")
        .WithSummary("Creates a new product")
        .WithDescription("Creates a new product in the catalog")
        .Produces<CreateProductResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireRoles(Roles.Admin);
    }
}