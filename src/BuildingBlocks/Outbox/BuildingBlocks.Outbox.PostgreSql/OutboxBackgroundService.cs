using BuildingBlocks.Outbox.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Outbox.PostgreSql;

internal sealed class OutboxBackgroundService(
    IOutboxSignal signal,
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<OutboxBackgroundService> logger) : BackgroundService
{
    private readonly OutboxOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox Processor Started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var outboxProcessor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

                var processedCount = await outboxProcessor.ProcessBatchAsync(stoppingToken);

                if (processedCount < _options.BatchSize)
                {
                    await signal.WaitForSignalAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Outbox loop");
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}