using BuildingBlocks.Core.CQRS;

namespace Inventory.API.Features.Stock.ReserveStock;

public record ReserveStockCommand(Dictionary<Guid, int> Items) : ICommand;