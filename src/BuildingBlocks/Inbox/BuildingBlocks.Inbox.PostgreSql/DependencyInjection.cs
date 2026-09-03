using BuildingBlocks.Core.Options;
using BuildingBlocks.Inbox.Abstractions;
using BuildingBlocks.Inbox.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Inbox.PostgreSql;

public static class DependencyInjection
{
    public static IServiceCollection AddInboxEntityFrameworkCore<TDbContext>(
        this IServiceCollection services, Action<InboxOptions>? configure = null) 
        where TDbContext : DbContext
    {
        services.AddOptions<InboxOptions>()
            .Configure(configure ?? (_ => { }))
            .ValidateFluentValidation()
            .ValidateOnStart();

        services.AddScoped<IInboxService, InboxService<TDbContext>>();
        return services;
    }

    public static IServiceCollection AddPostgreSqlInbox<TDbContext>(
        this IServiceCollection services, Action<InboxOptions>? configure = null)
        where TDbContext : DbContext
    {
        services.AddInboxEntityFrameworkCore<TDbContext>(configure);
        services.AddScoped<InboxProcessor<TDbContext>>();
        services.AddHostedService<InboxBackgroundService<TDbContext>>();
        services.AddHostedService<InboxCleanupService>();
        return services;
    }
}