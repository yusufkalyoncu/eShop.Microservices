using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web;

namespace Catalog.API.Features.Products.CreateProduct;

public sealed class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (CreateProductCommand command, ICommandHandler<CreateProductCommand, Guid> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);

            return result.IsSuccess 
                ? Results.Created($"/products/{result.Data}", result.Data) 
                : Results.BadRequest(result.Error);
        })
        .WithTags("Products")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}