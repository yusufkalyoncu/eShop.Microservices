namespace BuildingBlocks.Messaging.Abstractions;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) 
        where T : IIntegrationEvent;

    Task PublishAsync(object @event, Type eventType, CancellationToken cancellationToken = default);
}