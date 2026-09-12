using BuildingBlocks.Messaging.Abstractions;
using MassTransit;

namespace BuildingBlocks.Messaging.MassTransit;

public sealed class MassTransitEventBus(IBus bus) : IEventBus
{
    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) 
        where T : IIntegrationEvent
    {
        return bus.Publish(@event, cancellationToken);
    }

    public Task PublishAsync(object @event, Type eventType, CancellationToken cancellationToken = default)
    {
        return bus.Publish(@event, eventType, cancellationToken);
    }
}