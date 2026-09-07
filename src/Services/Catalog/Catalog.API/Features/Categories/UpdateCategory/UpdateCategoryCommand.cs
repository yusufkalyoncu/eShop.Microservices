using BuildingBlocks.Core.CQRS;

using BuildingBlocks.Core.Caching;

namespace Catalog.API.Features.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(Guid Id, string Name, string Description) : ICommand, ICacheInvalidatorCommand
{
    public IEnumerable<string> CacheKeys => ["GetCategories"];
}