namespace BuildingBlocks.Messaging.Abstractions;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) 
        where T : IIntegrationEvent;
}