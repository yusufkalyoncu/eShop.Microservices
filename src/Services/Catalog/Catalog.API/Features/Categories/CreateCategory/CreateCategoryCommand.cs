using BuildingBlocks.Core.CQRS;

namespace Catalog.API.Features.Categories.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string Description) : ICommand<Guid>;