using BuildingBlocks.Core.CQRS;

using BuildingBlocks.Core.Caching;

namespace Catalog.API.Features.Categories.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand, ICacheInvalidatorCommand
{
    public IEnumerable<string> CacheKeys => ["GetCategories"];
}