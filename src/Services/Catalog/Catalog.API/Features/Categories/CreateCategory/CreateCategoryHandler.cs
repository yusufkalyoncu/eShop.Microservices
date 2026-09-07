using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.ValueObjects;
using Catalog.API.Infrastructure.Data;

namespace Catalog.API.Features.Categories.CreateCategory;

internal sealed class CreateCategoryHandler(CatalogDbContext dbContext) : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var categoryName = CategoryName.Create(request.Name);
        var categoryDescription = CategoryDescription.Create(request.Description);

        var category = Category.Create(categoryName, categoryDescription);

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}