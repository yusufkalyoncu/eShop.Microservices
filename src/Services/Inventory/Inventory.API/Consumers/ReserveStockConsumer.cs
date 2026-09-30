using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Messaging.Abstractions;
using Inventory.Contracts.IntegrationEvents;

namespace Inventory.API.Consumers;

public class ReserveStockConsumer(ICommandHandler<Features.Stock.ReserveStock.ReserveStockCommand> handler, IEventBus eventBus) : IIntegrationEventHandler<Contracts.IntegrationCommands.ReserveStockCommand>
{
    public async Task HandleAsync(Contracts.IntegrationCommands.ReserveStockCommand @event, CancellationToken cancellationToken)
    {
        var command = new Features.Stock.ReserveStock.ReserveStockCommand(@event.Items);
        
        try
        {
            var result = await handler.Handle(command, cancellationToken);
            
            if (result.IsSuccess)
            {
                await eventBus.PublishAsync(new StockReservedEvent
                {
                    OrderId = @event.OrderId
                }, cancellationToken);
            }
            else
            {
                await eventBus.PublishAsync(new StockReservationFailedEvent
                {
                    OrderId = @event.OrderId,
                    Reason = result.Error.Description
                }, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            await eventBus.PublishAsync(new StockReservationFailedEvent
            {
                OrderId = @event.OrderId,
                Reason = ex.Message
            }, cancellationToken);
        }
    }
}