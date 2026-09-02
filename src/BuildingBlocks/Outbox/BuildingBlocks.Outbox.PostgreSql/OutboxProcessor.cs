using System.Collections.Concurrent;
using System.Text.Json;
using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Outbox.Abstractions;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BuildingBlocks.Outbox.PostgreSql;

internal sealed class OutboxProcessor(
    NpgsqlDataSource dataSource,
    IEventBus eventBus,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor> logger)
{

    private readonly OutboxOptions _options = options.Value;

    private static readonly TimeSpan[] BackoffSchedule =
    [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromMinutes(30)
    ];

    private static DateTime? CalculateNextAttempt(int retryCount)
    {
        var index = Math.Min(Math.Max(0, retryCount - 1), BackoffSchedule.Length - 1);
        var baseDelay = BackoffSchedule[index];
        
        var jitterMs = Random.Shared.Next(-20, 21) * baseDelay.TotalMilliseconds / 100;
        return DateTime.UtcNow.Add(baseDelay).AddMilliseconds(jitterMs);
    }

    
    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        try
        {
            List<OutboxMessage> messages;
            await using (var connection = await dataSource.OpenConnectionAsync(cancellationToken))
            {
                var sql = """
                          -- 1. True Head Pattern & Advisory Locks for partition ordering
                          WITH partition_heads AS (
                              SELECT DISTINCT ON (partition_key)
                                  partition_key, locked_until_utc, next_attempt_at_utc
                              FROM outbox_messages
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
                              WHERE pg_try_advisory_xact_lock(hashtext('outbox'), hashtext(partition_key))
                              LIMIT @BatchSize
                          ),
                          candidate_batch AS (
                              SELECT id
                              FROM outbox_messages
                              WHERE status = 0
                                AND (locked_until_utc IS NULL OR locked_until_utc < @Now)
                                AND (next_attempt_at_utc IS NULL OR next_attempt_at_utc <= @Now)
                                AND (partition_key IS NULL OR partition_key IN (SELECT partition_key FROM locked_partitions))
                              ORDER BY occurred_on_utc
                              LIMIT @BatchSize
                              FOR UPDATE SKIP LOCKED
                          )
                          UPDATE outbox_messages m
                          SET locked_until_utc = @LockExpiration
                          FROM candidate_batch c
                          WHERE m.id = c.id
                          RETURNING m.id AS Id, m.type AS Type, m.content AS Content, m.partition_key AS PartitionKey,
                                    m.occurred_on_utc AS OccurredOnUtc, m.retry_count AS RetryCount;
                          """;

                messages = (await connection.QueryAsync<OutboxMessage>(
                    sql,
                    new
                    {
                        LockExpiration = DateTime.UtcNow.Add(_options.LockTimeout),
                        Now = DateTime.UtcNow,
                        _options.BatchSize
                    }))
                    .OrderBy(x => x.OccurredOnUtc)
                    .ToList();
            }

            if (messages.Count == 0) return 0;

            var updateQueue = new ConcurrentQueue<OutboxUpdateResult>();
            var releaseQueue = new ConcurrentQueue<Guid>();
            
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            };

            var groups = messages
                .GroupBy(m => m.PartitionKey ?? m.Id.ToString())
                .ToList();

            await Parallel.ForEachAsync(groups, parallelOptions, async (group, ct) =>
            {
                var ordered = group.OrderBy(m => m.OccurredOnUtc).ToList();
                for (var i = 0; i < ordered.Count; i++)
                {
                    var succeeded = await PublishMessageAsync(ordered[i], updateQueue, ct);
                    if (succeeded) continue;

                    // True Head check ensures this partition won't be picked up again until this failed message retries,
                    // so it is safe to immediately release locks for the rest of the batch to avoid 'lock leak' stalls.
                    for (var j = i + 1; j < ordered.Count; j++)
                    {
                        releaseQueue.Enqueue(ordered[j].Id);
                    }
                    break;
                }
            });

            if (!releaseQueue.IsEmpty)
            {
                await using var releaseConnection = await dataSource.OpenConnectionAsync(cancellationToken);
                await releaseConnection.ExecuteAsync(
                    "UPDATE outbox_messages SET locked_until_utc = NULL WHERE id = ANY(@Ids) AND status = 0",
                    new { Ids = releaseQueue.ToArray() });
            }

            if (!updateQueue.IsEmpty)
            {
                var results = updateQueue.ToArray();

                await using var updateConnection = await dataSource.OpenConnectionAsync(cancellationToken);

                var updateSql = """
                                UPDATE outbox_messages AS m
                                SET processed_on_utc = u.processed_on_utc,
                                    error = u.error,
                                    retry_count = u.retry_count,
                                    status = u.status,
                                    next_attempt_at_utc = u.next_attempt_at_utc,
                                    locked_until_utc = NULL
                                FROM (
                                    SELECT * FROM UNNEST(
                                        @Ids::uuid[], @Dates::timestamp[], @Errors::text[], 
                                        @RetryCounts::integer[], @Statuses::integer[], @NextAttempts::timestamp[]
                                    ) AS t(id, processed_on_utc, error, retry_count, status, next_attempt_at_utc) 
                                ) AS u
                                WHERE m.id = u.id
                                """;

                await updateConnection.ExecuteAsync(updateSql, new
                {
                    Ids = results.Select(x => x.Id).ToArray(),
                    Dates = results.Select(x => x.ProcessedDate).ToArray(),
                    Errors = results.Select(x => x.Error).ToArray(),
                    RetryCounts = results.Select(x => x.RetryCount).ToArray(),
                    Statuses = results.Select(x => (int)x.Status).ToArray(),
                    NextAttempts = results.Select(x => x.NextAttemptAtUtc).ToArray()
                });
            }

            logger.LogInformation("{Count} messages processed.", messages.Count);
            return messages.Count;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Batch processing failed.");
            throw;
        }
    }

    private async Task<bool> PublishMessageAsync(
        OutboxMessage message,
        ConcurrentQueue<OutboxUpdateResult> resultQueue,
        CancellationToken ct)
    {
        string? error = null;
        DateTime? processedDate = null;
        int currentRetryCount = message.RetryCount;

        try
        {
            var msgType = OutboxEventTypeResolver.GetEventType(message.Type);

            if (msgType != null)
            {
                var content = JsonSerializer.Deserialize(message.Content, msgType, OutboxJsonOptions.Default);

                if (content is IIntegrationEvent integrationEvent)
                {
                    await eventBus.PublishAsync(integrationEvent, msgType, ct);
                    processedDate = DateTime.UtcNow;
                }
                else
                {
                    error = $"Content is null or not IIntegrationEvent. Type: {message.Type}";
                    currentRetryCount++;
                }
            }
            else
            {
                error = $"Type not found: {message.Type}";
                currentRetryCount++;
            }
        }
        catch (Exception ex)
        {
            error = ex.ToString();
            currentRetryCount++;
        }

        if (error != null)
        {
            var status = currentRetryCount >= _options.MaxRetryCount 
                ? OutboxMessageStatus.DeadLettered 
                : OutboxMessageStatus.Pending;

            var nextAttempt = status == OutboxMessageStatus.Pending 
                ? CalculateNextAttempt(currentRetryCount) 
                : null;

            if (status == OutboxMessageStatus.DeadLettered)
            {
                logger.LogWarning(
                    "Outbox message {Id} of type {Type} dead-lettered after {RetryCount} attempts. Last error: {Error}",
                    message.Id, message.Type, currentRetryCount, error);
            }

            resultQueue.Enqueue(new OutboxUpdateResult(message.Id, null, error, currentRetryCount, status, nextAttempt));
            return false;
        }

        resultQueue.Enqueue(new OutboxUpdateResult(message.Id, processedDate, null, currentRetryCount, OutboxMessageStatus.Processed, null));
        return true;
    }

    private readonly record struct OutboxUpdateResult(Guid Id, DateTime? ProcessedDate, string? Error, int RetryCount, OutboxMessageStatus Status, DateTime? NextAttemptAtUtc);
}