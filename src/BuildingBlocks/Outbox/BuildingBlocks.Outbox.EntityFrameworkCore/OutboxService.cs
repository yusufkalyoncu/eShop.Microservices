using System.Text.Json;
using BuildingBlocks.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Outbox.EntityFrameworkCore;

public sealed class OutboxService<TContext>(TContext dbContext) : IOutboxService
    where TContext : DbContext
{
    public async Task AddAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        var runtimeType = message.GetType();

        var typeName = $"{runtimeType.FullName}, {runtimeType.Assembly.GetName().Name}";

        var jsonContent = JsonSerializer.Serialize(message, runtimeType, OutboxJsonOptions.Default);

        var outboxMessage = new OutboxMessage(typeName, jsonContent);

        await dbContext.Set<OutboxMessage>().AddAsync(outboxMessage, cancellationToken);
    }
}