using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Pagination;
using BuildingBlocks.Core.Results;
using BuildingBlocks.Persistence.EntityFrameworkCore.Pagination;
using Catalog.API.Infrastructure.Data;

namespace Catalog.API.Features.Products.GetProducts;

internal sealed class GetProductsHandler(CatalogDbContext dbContext) : IQueryHandler<GetProductsQuery, PaginatedResult<ProductDto>>
{
    public async Task<Result<PaginatedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var productsQuery = dbContext.Products
            .Select(p => new ProductDto(
                p.Id, 
                p.Name.Value, 
                p.Description.Value, 
                p.Price.Amount, 
                p.Price.Currency, 
                p.CategoryId));

        var paginatedProducts = await productsQuery.ToPaginatedResultAsync(request.PageIndex, request.PageSize, cancellationToken);

        return paginatedProducts;
    }
}