using BuildingBlocks.Core.CQRS;

namespace Inventory.API.Features.Stock.GetStock;

public record GetStockQuery(Guid ProductId) : IQuery<GetStockResult>;
public record GetStockResult(Guid ProductId, int AvailableQuantity);