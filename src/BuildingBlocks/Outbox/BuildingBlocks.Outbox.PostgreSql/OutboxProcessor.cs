using System.Collections.Concurrent;
using System.Text.Json;
using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Outbox.Abstractions;
using BuildingBlocks.Outbox.EntityFrameworkCore;
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
    private static readonly ConcurrentDictionary<string, Type?> TypeCache = new();
    private readonly OutboxOptions _options = options.Value;

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var messages = (await connection.QueryAsync<OutboxMessage>(
                """
                SELECT 
                    id AS Id, 
                    type AS Type, 
                    content AS Content,
                    retry_count AS RetryCount
                FROM outbox_messages
                WHERE processed_on_utc IS NULL AND retry_count < @MaxRetryCount
                ORDER BY occurred_on_utc
                LIMIT @BatchSize
                FOR UPDATE SKIP LOCKED
                """,
                new { _options.BatchSize, _options.MaxRetryCount },
                transaction: transaction
            )).ToList();

            if (messages.Count == 0)
            {
                await transaction.CommitAsync(cancellationToken);
                return 0;
            }

            var updateQueue = new ConcurrentQueue<OutboxUpdateResult>();

            var parallelOptions = new ParallelOptions 
            { 
                MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism,
                CancellationToken = cancellationToken 
            };

            await Parallel.ForEachAsync(messages, parallelOptions, async (msg, ct) =>
            {
                await PublishMessageAsync(msg, updateQueue, ct);
            });

            if (!updateQueue.IsEmpty)
            {
                var results = updateQueue.ToArray();

                var sql = """
                          UPDATE outbox_messages AS m
                          SET processed_on_utc = u.processed_on_utc,
                              error = u.error,
                              retry_count = u.retry_count
                          FROM (
                              SELECT * FROM UNNEST(@Ids, @Dates, @Errors, @RetryCounts) 
                              AS t(id, processed_on_utc, error, retry_count) 
                          ) AS u
                          WHERE m.id = u.id
                          """;

                await connection.ExecuteAsync(sql, new
                {
                    Ids = results.Select(x => x.Id).ToArray(),
                    Dates = results.Select(x => x.ProcessedDate).ToArray(),
                    Errors = results.Select(x => x.Error).ToArray(),
                    RetryCounts = results.Select(x => x.RetryCount).ToArray()
                }, transaction: transaction);
            }

            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("{Count} messages processed.", messages.Count);
            return messages.Count;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Batch processing failed.");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task PublishMessageAsync(
        OutboxMessage message,
        ConcurrentQueue<OutboxUpdateResult> resultQueue,
        CancellationToken ct)
    {
        string? error = null;
        DateTime? processedDate = null;
        int currentRetryCount = message.RetryCount;

        try
        {
            var msgType = GetMessageType(message.Type);

            if (msgType != null)
            {
                var content = JsonSerializer.Deserialize(message.Content, msgType, OutboxJsonOptions.Default);

                if (content is IIntegrationEvent integrationEvent)
                {
                    await eventBus.PublishAsync(integrationEvent, ct);
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

        resultQueue.Enqueue(new OutboxUpdateResult(message.Id, processedDate, error, currentRetryCount));
    }

    private static Type? GetMessageType(string typeName)
    {
        return TypeCache.GetOrAdd(typeName, type =>
        {
            var resolvedType = Type.GetType(type);
            if (resolvedType != null) return resolvedType;

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName == type || t.Name == type);
        });
    }

    private readonly record struct OutboxUpdateResult(Guid Id, DateTime? ProcessedDate, string? Error, int RetryCount);
}