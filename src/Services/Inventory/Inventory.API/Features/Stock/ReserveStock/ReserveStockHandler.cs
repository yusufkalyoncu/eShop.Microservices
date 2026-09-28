using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Results;
using Inventory.API.Domain.Errors;
using Inventory.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Features.Stock.ReserveStock;

internal sealed class ReserveStockHandler(InventoryDbContext dbContext) : ICommandHandler<ReserveStockCommand>
{
    public async Task<Result> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        var item = await dbContext.InventoryItems
            .FirstOrDefaultAsync(x => x.ProductId == request.ProductId, cancellationToken);

        if (item is null)
        {
            throw new DomainException(InventoryErrors.ProductNotFound(request.ProductId));
        }

        item.ReserveStock(request.Quantity);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}