using BuildingBlocks.Core.CQRS;

namespace Catalog.API.Features.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(Guid Id, string Name, string Description) : ICommand;