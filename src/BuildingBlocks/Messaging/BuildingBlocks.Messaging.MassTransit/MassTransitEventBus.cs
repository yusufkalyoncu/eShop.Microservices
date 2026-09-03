using BuildingBlocks.Messaging.Abstractions;
using MassTransit;

namespace BuildingBlocks.Messaging.MassTransit;

public sealed class MassTransitEventBus(IPublishEndpoint publishEndpoint) : IEventBus
{
    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) 
        where T : IIntegrationEvent
    {
        return publishEndpoint.Publish(@event, cancellationToken);
    }

    public Task PublishAsync(object @event, Type eventType, CancellationToken cancellationToken = default)
    {
        return publishEndpoint.Publish(@event, eventType, cancellationToken);
    }
}