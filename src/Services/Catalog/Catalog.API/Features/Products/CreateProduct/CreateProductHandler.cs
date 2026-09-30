using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.Errors;
using Catalog.API.Domain.ValueObjects;
using Catalog.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Features.Products.CreateProduct;

internal sealed class CreateProductHandler(CatalogDbContext dbContext) : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await dbContext.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            return Result.Failure<Guid>(CatalogErrors.Category.NotFound);
        }

        var productName = ProductName.Create(request.Name);
        var productDescription = ProductDescription.Create(request.Description);
        var productPrice = Money.Create(request.Price);

        var product = Product.Create(productName, productDescription, productPrice, request.CategoryId);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}