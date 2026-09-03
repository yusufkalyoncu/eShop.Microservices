using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Catalog.API.Domain.Entities;
using Catalog.API.Infrastructure.Data;

namespace Catalog.API.Features.Products.CreateProduct;

internal sealed class CreateProductHandler(CatalogDbContext dbContext) : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Description, request.Price);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}