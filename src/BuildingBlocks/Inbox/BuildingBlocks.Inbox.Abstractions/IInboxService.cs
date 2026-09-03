using BuildingBlocks.Messaging.Abstractions;

namespace BuildingBlocks.Inbox.Abstractions;

public interface IInboxService
{
    Task<bool> TryAddAsync<TEvent>(TEvent @event, string? partitionKey = null, CancellationToken cancellationToken = default) 
        where TEvent : class, IIntegrationEvent;
}