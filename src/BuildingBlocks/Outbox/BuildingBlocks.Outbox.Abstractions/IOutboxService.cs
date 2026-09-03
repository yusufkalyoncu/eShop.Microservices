using BuildingBlocks.Messaging.Abstractions;

namespace BuildingBlocks.Outbox.Abstractions;

public interface IOutboxService
{
    Task AddAsync<T>(T message, string? partitionKey = null, CancellationToken cancellationToken = default) where T : class, IIntegrationEvent;
}