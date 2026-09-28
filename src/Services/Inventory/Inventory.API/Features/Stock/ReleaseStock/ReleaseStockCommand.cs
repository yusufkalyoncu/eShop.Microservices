using BuildingBlocks.Core.CQRS;

namespace Inventory.API.Features.Stock.ReleaseStock;

public record ReleaseStockCommand(Guid ProductId, int Quantity) : ICommand;