using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Catalog.API.Features.Products.CreateProduct;

public sealed class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products",async (CreateProductCommand command, ICommandHandler<CreateProductCommand, Guid> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.Match();
        })
        .WithTags("Products")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}