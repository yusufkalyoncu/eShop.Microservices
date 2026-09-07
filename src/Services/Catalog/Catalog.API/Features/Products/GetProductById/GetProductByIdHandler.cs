using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Catalog.API.Domain.Errors;
using Catalog.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Features.Products.GetProductById;

internal sealed class GetProductByIdHandler(CatalogDbContext dbContext) : IQueryHandler<GetProductByIdQuery, ProductDetailsDto>
{
    public async Task<Result<ProductDetailsDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Join(dbContext.Categories, 
                p => p.CategoryId, 
                c => c.Id, 
                (p, c) => new ProductDetailsDto(
                    p.Id, 
                    p.Name.Value, 
                    p.Description.Value, 
                    p.Price.Amount, 
                    p.Price.Currency, 
                    p.CategoryId,
                    c.Name.Value))
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductDetailsDto>(CatalogErrors.Product.NotFound);
        }

        return product;
    }
}