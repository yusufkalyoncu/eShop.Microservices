using BuildingBlocks.Core.CQRS;

namespace Catalog.API.Features.Categories.GetCategories;

public sealed record GetCategoriesQuery : IQuery<IReadOnlyList<CategoryDto>>;