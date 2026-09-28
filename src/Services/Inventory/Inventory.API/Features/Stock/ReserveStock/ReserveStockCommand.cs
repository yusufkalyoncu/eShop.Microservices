using BuildingBlocks.Core.CQRS;

namespace Inventory.API.Features.Stock.ReserveStock;

public record ReserveStockCommand(Guid ProductId, int Quantity) : ICommand;