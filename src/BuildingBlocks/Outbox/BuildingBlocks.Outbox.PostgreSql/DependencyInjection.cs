using BuildingBlocks.Outbox.Abstractions;
using BuildingBlocks.Outbox.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Outbox.PostgreSql;

public static class DependencyInjection
{
    public static IServiceCollection AddPostgreSqlOutbox<TDbContext>(
        this IServiceCollection services,
        Action<OutboxOptions>? configure = null)
        where TDbContext : DbContext
    {
        services.AddOutboxEntityFrameworkCore<TDbContext>(configure);
        
        services.AddScoped<OutboxProcessor>();
        services.AddHostedService<OutboxBackgroundService>();

        return services;
    }
}