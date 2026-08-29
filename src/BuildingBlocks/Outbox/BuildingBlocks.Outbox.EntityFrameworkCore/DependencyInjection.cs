using BuildingBlocks.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Outbox.EntityFrameworkCore;

public static class DependencyInjection
{
    public static IServiceCollection AddOutboxEntityFrameworkCore<TDbContext>(
        this IServiceCollection services,
        Action<OutboxOptions>? configure = null) 
        where TDbContext : DbContext
    {
        services.AddOptions<OutboxOptions>()
            .Configure(configure ?? (_ => { }))
            .Validate(o => o.BatchSize is > 0 and <= 1000)
            .Validate(o => o.MaxRetryCount is >= 0 and <= 20)
            .Validate(o => o.MaxDegreeOfParallelism is > 0 and <= 100)
            .ValidateOnStart();

        services.AddScoped<IOutboxService, OutboxService<TDbContext>>();
        services.AddScoped<OutboxInsertInterceptor>();
        services.AddSingleton<IOutboxSignal, OutboxSignal>();

        return services;
    }
}