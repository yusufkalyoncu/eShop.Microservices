using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Messaging.Abstractions;

namespace Inventory.API.Consumers;

public class ReleaseStockConsumer(ICommandHandler<Features.Stock.ReleaseStock.ReleaseStockCommand> handler) : IIntegrationEventHandler<Contracts.IntegrationCommands.ReleaseStockCommand>
{
    public async Task HandleAsync(Contracts.IntegrationCommands.ReleaseStockCommand @event, CancellationToken cancellationToken)
    {
        var command = new Features.Stock.ReleaseStock.ReleaseStockCommand(@event.Items);
        
        await handler.Handle(command, cancellationToken);
    }
}