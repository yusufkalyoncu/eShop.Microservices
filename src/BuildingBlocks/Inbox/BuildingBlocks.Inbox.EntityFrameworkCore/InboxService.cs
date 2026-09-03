using System.Text.Json;
using BuildingBlocks.Inbox.Abstractions;
using BuildingBlocks.Messaging.Abstractions;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Inbox.EntityFrameworkCore;

public sealed class InboxService<TContext>(TContext dbContext, ILogger<InboxService<TContext>> logger) 
    : IInboxService where TContext : DbContext
{
    public async Task<bool> TryAddAsync<TEvent>(TEvent @event, string? partitionKey = null, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        var runtimeType = @event.GetType();
        var jsonContent = JsonSerializer.Serialize(@event, runtimeType, InboxJsonOptions.Default);
        var typeName = TEvent.EventName;

        var inboxMessage = new InboxMessage(@event.EventId, typeName, jsonContent, DateTime.UtcNow, partitionKey);
        dbContext.Set<InboxMessage>().Add(inboxMessage);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (UniqueConstraintException)
        {
            logger.LogDebug("Inbox message {Id} already exists, skipping (duplicate delivery).", @event.EventId);
            dbContext.Entry(inboxMessage).State = EntityState.Detached;
            return false;
        }
    }
}