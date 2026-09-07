using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Catalog.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Features.Categories.GetCategories;

internal sealed class GetCategoriesHandler(CatalogDbContext dbContext) : IQueryHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .Select(c => new CategoryDto(c.Id, c.Name.Value, c.Description.Value))
            .ToListAsync(cancellationToken);

        return categories;
    }
}