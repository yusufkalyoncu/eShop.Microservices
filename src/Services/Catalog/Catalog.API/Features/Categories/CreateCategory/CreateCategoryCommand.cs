using BuildingBlocks.Core.CQRS;

using BuildingBlocks.Core.Caching;

namespace Catalog.API.Features.Categories.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string Description) : ICommand<Guid>, ICacheInvalidatorCommand
{
    public IEnumerable<string> CacheKeys => ["GetCategories"];
}