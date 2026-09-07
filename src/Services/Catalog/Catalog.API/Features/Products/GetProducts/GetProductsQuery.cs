using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Pagination;

namespace Catalog.API.Features.Products.GetProducts;

public sealed record GetProductsQuery(int PageIndex = 1, int PageSize = 10) : IQuery<PaginatedResult<ProductDto>>;