using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Catalog.API.Domain.Errors;
using Catalog.API.Domain.ValueObjects;
using Catalog.API.Infrastructure.Data;

namespace Catalog.API.Features.Categories.UpdateCategory;

internal sealed class UpdateCategoryHandler(CatalogDbContext dbContext) : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.FindAsync([request.Id], cancellationToken);

        if (category is null)
        {
            return Result.Failure(CatalogErrors.Category.NotFound);
        }

        var categoryName = CategoryName.Create(request.Name);
        var categoryDescription = CategoryDescription.Create(request.Description);

        category.Update(categoryName, categoryDescription);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}