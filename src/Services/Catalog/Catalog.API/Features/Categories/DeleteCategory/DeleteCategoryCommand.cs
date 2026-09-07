using BuildingBlocks.Core.CQRS;

namespace Catalog.API.Features.Categories.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand;