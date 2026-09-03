using System.Collections.Concurrent;
using System.Text.Json;
using BuildingBlocks.Inbox.Abstractions;
using BuildingBlocks.Messaging.Abstractions;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BuildingBlocks.Inbox.PostgreSql;

internal sealed class InboxProcessor<TDbContext>(
    NpgsqlDataSource dataSource,
    IServiceScopeFactory scopeFactory,
    IOptions<InboxOptions> options,
    ILogger<InboxProcessor<TDbContext>> logger)
    where TDbContext : DbContext
{
    private readonly InboxOptions _options = options.Value;

    public async Task<(int Processed, int Claimed)> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        List<InboxMessage> messages;

        await using (var connection = await dataSource.OpenConnectionAsync(cancellationToken))
        {
            var sql = """
                      WITH partition_heads AS (
                          SELECT DISTINCT ON (partition_key)
                              partition_key, locked_until_utc, next_attempt_at_utc
                          FROM inbox_messages
                          WHERE status = 0 AND partition_key IS NOT NULL
                          ORDER BY partition_key, occurred_on_utc, id
                      ),
                      ready_partitions AS (
                          SELECT partition_key FROM partition_heads
                          WHERE (locked_until_utc IS NULL OR locked_until_utc < @Now)
                            AND (next_attempt_at_utc IS NULL OR next_attempt_at_utc <= @Now)
                      ),
                      locked_partitions AS (
                          SELECT partition_key
                          FROM ready_partitions
                          WHERE pg_try_advisory_xact_lock(hashtext('inbox'), hashtext(partition_key))
                          LIMIT @BatchSize
                      ),
                      candidate_batch AS (
                          SELECT id
                          FROM inbox_messages
                          WHERE status = 0
                            AND (locked_until_utc IS NULL OR locked_until_utc < @Now)
                            AND (next_attempt_at_utc IS NULL OR next_attempt_at_utc <= @Now)
                            AND (partition_key IS NULL OR partition_key IN (SELECT partition_key FROM locked_partitions))
                          ORDER BY occurred_on_utc
                          LIMIT @BatchSize
                          FOR UPDATE SKIP LOCKED
                      )
                      UPDATE inbox_messages m
                      SET locked_until_utc = @LockExpiration
                      FROM candidate_batch c
                      WHERE m.id = c.id
                      RETURNING m.id AS Id, m.type AS Type, m.content AS Content, m.partition_key AS PartitionKey,
                                m.occurred_on_utc AS OccurredOnUtc, m.retry_count AS RetryCount;
                      """;

            messages = (await connection.QueryAsync<InboxMessage>(sql, new
            {
                Now = DateTime.UtcNow,
                LockExpiration = DateTime.UtcNow.Add(_options.LockTimeout),
                _options.BatchSize
            })).OrderBy(x => x.OccurredOnUtc).ToList();
        }

        var claimedCount = messages.Count;
        if (claimedCount == 0) return (0, 0);

        var releaseQueue = new ConcurrentQueue<Guid>();
        var processedCount = 0;

        var groups = messages.GroupBy(m => m.PartitionKey ?? m.Id.ToString()).ToList();
        var parallelOptions = new ParallelOptions 
        { 
            MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism, 
            CancellationToken = cancellationToken 
        };

        await Parallel.ForEachAsync(groups, parallelOptions, async (group, ct) =>
        {
            var ordered = group.OrderBy(m => m.OccurredOnUtc).ToList();

            for (var i = 0; i < ordered.Count; i++)
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var succeeded = await ProcessSingleMessageAsync(scope.ServiceProvider, ordered[i], ct);
                
                if (succeeded) Interlocked.Increment(ref processedCount);

                if (!succeeded)
                {
                    for (var j = i + 1; j < ordered.Count; j++)
                        releaseQueue.Enqueue(ordered[j].Id);
                    break;
                }
            }
        });

        if (!releaseQueue.IsEmpty)
        {
            await using var releaseConnection = await dataSource.OpenConnectionAsync(cancellationToken);
            await releaseConnection.ExecuteAsync(
                "UPDATE inbox_messages SET locked_until_utc = NULL WHERE id = ANY(@Ids) AND status = 0",
                new { Ids = releaseQueue.ToArray() });
        }

        logger.LogInformation("{Count} inbox messages processed out of {Claimed} claimed.", processedCount, claimedCount);
        return (processedCount, claimedCount);
    }

    private async Task<bool> ProcessSingleMessageAsync(IServiceProvider serviceProvider, InboxMessage message, CancellationToken ct)
    {
        var dbContext = serviceProvider.GetRequiredService<TDbContext>();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            var msgType = InboxEventTypeResolver.GetEventType(message.Type);
            if (msgType == null)
            {
                await transaction.RollbackAsync(ct);
                await FailAsync(message, $"Type not found: {message.Type}", ct);
                return false;
            }

            var content = JsonSerializer.Deserialize(message.Content, msgType, InboxJsonOptions.Default);
            if (content is not IIntegrationEvent integrationEvent)
            {
                await transaction.RollbackAsync(ct);
                await FailAsync(message, $"Content is not IIntegrationEvent. Type: {message.Type}", ct);
                return false;
            }

            var handled = await InvokeHandlersAsync(serviceProvider, msgType, integrationEvent, ct);
            if (!handled)
            {
                logger.LogDebug("No handler registered for {Type}, marking as processed (no-op).", message.Type);
            }

            var tracked = InboxMessage.Rehydrate(message.Id, message.Type, message.Content, message.OccurredOnUtc, message.PartitionKey, message.RetryCount);
            dbContext.Set<InboxMessage>().Attach(tracked);
            tracked.MarkProcessed(DateTime.UtcNow);

            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            await FailAsync(message, ex.ToString(), ct);
            return false;
        }
    }

    private async Task<bool> InvokeHandlersAsync(IServiceProvider serviceProvider, Type eventType, IIntegrationEvent @event, CancellationToken ct)
    {
        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handlers = serviceProvider.GetServices(handlerType).ToList();

        if (handlers.Count == 0) return false;

        foreach (var handler in handlers)
        {
            dynamic dynamicHandler = handler!;
            dynamic dynamicEvent = @event;
            await dynamicHandler.HandleAsync(dynamicEvent, ct);
        }

        return true;
    }

    private async Task FailAsync(InboxMessage message, string error, CancellationToken ct)
    {
        await using var errorScope = scopeFactory.CreateAsyncScope();
        var dbContext = errorScope.ServiceProvider.GetRequiredService<TDbContext>();

        var nextAttempt = CalculateNextAttempt(message.RetryCount + 1);
        var tracked = InboxMessage.Rehydrate(message.Id, message.Type, message.Content, message.OccurredOnUtc, message.PartitionKey, message.RetryCount);
        dbContext.Set<InboxMessage>().Attach(tracked);
        
        tracked.MarkFailed(error, _options.MaxRetryCount, nextAttempt);

        if (tracked.Status == InboxMessageStatus.DeadLettered)
        {
            logger.LogWarning("Inbox message {Id} of type {Type} dead-lettered after {RetryCount} attempts. Last error: {Error}",
                message.Id, message.Type, tracked.RetryCount, error);
        }

        await dbContext.SaveChangesAsync(ct);
    }

    private static readonly TimeSpan[] BackoffSchedule =
    [
        TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2), 
        TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(30)
    ];

    private static DateTime? CalculateNextAttempt(int retryCount)
    {
        var index = Math.Min(Math.Max(0, retryCount - 1), BackoffSchedule.Length - 1);
        var jitterMs = Random.Shared.Next(-20, 21) * BackoffSchedule[index].TotalMilliseconds / 100;
        return DateTime.UtcNow.Add(BackoffSchedule[index]).AddMilliseconds(jitterMs);
    }
}
