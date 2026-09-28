using BuildingBlocks.Core.CQRS;

namespace Inventory.API.Features.Stock.AddStock;

public record AddStockCommand(Guid ProductId, int Quantity) : ICommand;