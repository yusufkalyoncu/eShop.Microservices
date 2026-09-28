using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Inventory.API.Domain.Entities;
using Inventory.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Features.Stock.AddStock;

internal sealed class AddStockHandler(InventoryDbContext dbContext) : ICommandHandler<AddStockCommand>
{
    public async Task<Result> Handle(AddStockCommand request, CancellationToken cancellationToken)
    {
        var item = await dbContext.InventoryItems
            .FirstOrDefaultAsync(x => x.ProductId == request.ProductId, cancellationToken);

        if (item is null)
        {
            item = InventoryItem.Create(request.ProductId, request.Quantity);
            dbContext.InventoryItems.Add(item);
        }
        else
        {
            item.AddStock(request.Quantity);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}