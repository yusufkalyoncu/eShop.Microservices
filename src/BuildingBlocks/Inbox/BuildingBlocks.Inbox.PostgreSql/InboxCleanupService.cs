using BuildingBlocks.Inbox.Abstractions;
using Dapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BuildingBlocks.Inbox.PostgreSql;

internal sealed class InboxCleanupService(
    NpgsqlDataSource dataSource, 
    IOptions<InboxOptions> options, 
    ILogger<InboxCleanupService> logger) 
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(6));
        do
        {
            try
            {
                var cutoff = DateTime.UtcNow.Subtract(options.Value.ProcessedMessageRetention);
                int deletedInBatch;

                do
                {
                    await using var connection = await dataSource.OpenConnectionAsync(stoppingToken);
                    deletedInBatch = await connection.ExecuteAsync("""
                        DELETE FROM inbox_messages 
                        WHERE id IN (
                            SELECT id FROM inbox_messages 
                            WHERE status = 1 AND processed_on_utc < @Cutoff 
                            LIMIT 10000
                        )
                        """, new { Cutoff = cutoff }, commandTimeout: 30);

                    if (deletedInBatch > 0)
                    {
                        logger.LogInformation("Cleaned up {Count} processed inbox messages.", deletedInBatch);
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    }
                } while (deletedInBatch > 0 && !stoppingToken.IsCancellationRequested);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Inbox cleanup failed.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}