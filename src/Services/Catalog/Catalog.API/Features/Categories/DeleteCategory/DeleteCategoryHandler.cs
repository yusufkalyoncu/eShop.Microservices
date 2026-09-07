using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Catalog.API.Domain.Errors;
using Catalog.API.Infrastructure.Data;

namespace Catalog.API.Features.Categories.DeleteCategory;

internal sealed class DeleteCategoryHandler(CatalogDbContext dbContext) : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.FindAsync([request.Id], cancellationToken);

        if (category is null)
        {
            return Result.Failure(CatalogErrors.Category.NotFound);
        }

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}