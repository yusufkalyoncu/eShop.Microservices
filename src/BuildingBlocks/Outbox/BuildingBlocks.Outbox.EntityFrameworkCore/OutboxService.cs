using System.Text.Json;
using BuildingBlocks.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Outbox.EntityFrameworkCore;

public sealed class OutboxService<TContext>(TContext dbContext) : IOutboxService
    where TContext : DbContext
{
    public async Task AddAsync<T>(T message, string? partitionKey = null, CancellationToken cancellationToken = default) 
        where T : class, IOutboxEvent
    {
        var runtimeType = message.GetType();

        var jsonContent = JsonSerializer.Serialize(message, runtimeType, OutboxJsonOptions.Default);

        var outboxMessage = new OutboxMessage(T.EventName, jsonContent, partitionKey);

        await dbContext.Set<OutboxMessage>().AddAsync(outboxMessage, cancellationToken);
    }
}