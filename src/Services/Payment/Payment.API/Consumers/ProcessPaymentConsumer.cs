using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Messaging.Abstractions;
using Payment.Contracts.IntegrationEvents;

namespace Payment.API.Consumers;

public class ProcessPaymentConsumer(ICommandHandler<Features.Payments.ProcessPayment.ProcessPaymentCommand> handler, IEventBus eventBus) : IIntegrationEventHandler<Contracts.IntegrationCommands.ProcessPaymentIntegrationCommand>
{
    public async Task HandleAsync(Contracts.IntegrationCommands.ProcessPaymentIntegrationCommand @event, CancellationToken cancellationToken)
    {
        var command = new Features.Payments.ProcessPayment.ProcessPaymentCommand(@event.OrderId, @event.Amount, @event.PaymentToken);
        
        try
        {
            var result = await handler.Handle(command, cancellationToken);
            
            if (result.IsSuccess)
            {
                await eventBus.PublishAsync(new PaymentSucceededEvent
                {
                    OrderId = @event.OrderId
                }, cancellationToken);
            }
            else
            {
                await eventBus.PublishAsync(new PaymentFailedEvent
                {
                    OrderId = @event.OrderId,
                    Reason = result.Error.Description
                }, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            await eventBus.PublishAsync(new PaymentFailedEvent
            {
                OrderId = @event.OrderId,
                Reason = ex.Message
            }, cancellationToken);
        }
    }
}