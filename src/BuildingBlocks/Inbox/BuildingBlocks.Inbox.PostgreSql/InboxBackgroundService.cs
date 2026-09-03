using BuildingBlocks.Inbox.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Inbox.PostgreSql;

internal sealed class InboxBackgroundService<TDbContext>(
    IServiceScopeFactory scopeFactory, 
    IOptions<InboxOptions> options, 
    ILogger<InboxBackgroundService<TDbContext>> logger) 
    : BackgroundService where TDbContext : DbContext
{
    private readonly InboxOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Inbox Processor Started for {DbContext}.", typeof(TDbContext).Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<InboxProcessor<TDbContext>>();
                var result = await processor.ProcessBatchAsync(stoppingToken);

                if (result.Claimed < _options.BatchSize)
                {
                    await Task.Delay(_options.PollTimeout, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Inbox loop");
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}