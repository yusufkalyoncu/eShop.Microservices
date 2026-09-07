using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Catalog.API.Domain.Errors;
using Catalog.API.Domain.ValueObjects;
using Catalog.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Features.Products.UpdateProduct;

internal sealed class UpdateProductHandler(CatalogDbContext dbContext) : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.FindAsync([request.Id], cancellationToken);

        if (product is null)
        {
            return Result.Failure(CatalogErrors.Product.NotFound);
        }

        var categoryExists = await dbContext.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            return Result.Failure(CatalogErrors.Category.NotFound);
        }

        var productName = ProductName.Create(request.Name);
        var productDescription = ProductDescription.Create(request.Description);
        var price = Money.Create(request.Price, request.Currency);

        product.UpdateDetails(productName, productDescription);
        product.UpdatePrice(price);
        product.ChangeCategory(request.CategoryId);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}