using System.Diagnostics;
using System.Text.Json;
using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Outbox.EntityFrameworkCore;

public sealed class OutboxService<TContext>(TContext dbContext) : IOutboxService
    where TContext : DbContext
{
    public async Task AddAsync<T>(T message, string? partitionKey = null, CancellationToken cancellationToken = default) 
        where T : class, IIntegrationEvent
    {
        var runtimeType = message.GetType();
        var jsonContent = JsonSerializer.Serialize(message, runtimeType, OutboxJsonOptions.Default);

        // Capture the current W3C traceparent so the OutboxProcessor can restore
        // this trace context when publishing to RabbitMQ, creating a single
        // end-to-end trace: HTTP Request → Outbox publish → RabbitMQ → Consumer.
        var traceParent = Activity.Current?.Id;

        var outboxMessage = new OutboxMessage(T.EventName, jsonContent, partitionKey, traceParent);
        await dbContext.Set<OutboxMessage>().AddAsync(outboxMessage, cancellationToken);
    }
}