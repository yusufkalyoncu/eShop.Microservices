using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Results;
using Inventory.API.Domain.Errors;
using Inventory.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Features.Stock.ReleaseStock;

internal sealed class ReleaseStockHandler(InventoryDbContext dbContext) : ICommandHandler<ReleaseStockCommand>
{
    public async Task<Result> Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.Items.Keys.ToList();
        
        var inventoryItems = await dbContext.InventoryItems
            .Where(x => productIds.Contains(x.ProductId))
            .ToListAsync(cancellationToken);

        foreach (var requestedItem in request.Items)
        {
            var item = inventoryItems.FirstOrDefault(x => x.ProductId == requestedItem.Key);
            if (item is null)
            {
                throw new DomainException(InventoryErrors.ProductNotFound(requestedItem.Key));
            }

            item.ReleaseStock(requestedItem.Value);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}