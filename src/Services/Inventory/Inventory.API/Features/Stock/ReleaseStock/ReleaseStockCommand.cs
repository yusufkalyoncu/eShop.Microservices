using BuildingBlocks.Core.CQRS;

namespace Inventory.API.Features.Stock.ReleaseStock;

public record ReleaseStockCommand(Dictionary<Guid, int> Items) : ICommand;