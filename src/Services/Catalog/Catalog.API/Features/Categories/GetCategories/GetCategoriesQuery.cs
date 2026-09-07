using BuildingBlocks.Core.CQRS;

using BuildingBlocks.Core.Caching;

namespace Catalog.API.Features.Categories.GetCategories;

public sealed record GetCategoriesQuery : IQuery<IReadOnlyList<CategoryDto>>, ICacheableQuery
{
    public string CacheKey => "GetCategories";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}