using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Inventory.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Features.Stock.GetStock;

internal sealed class GetStockHandler(InventoryDbContext dbContext) : IQueryHandler<GetStockQuery, GetStockResult>
{
    public async Task<Result<GetStockResult>> Handle(GetStockQuery request, CancellationToken cancellationToken)
    {
        var item = await dbContext.InventoryItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == request.ProductId, cancellationToken);

        if (item is null)
        {
            return new GetStockResult(request.ProductId, 0); // If not tracked, assume 0
        }

        return new GetStockResult(item.ProductId, item.AvailableQuantity.Value);
    }
}