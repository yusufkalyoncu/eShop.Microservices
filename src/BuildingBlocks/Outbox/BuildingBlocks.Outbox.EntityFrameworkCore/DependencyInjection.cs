using BuildingBlocks.Core.Options;
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
            .ValidateFluentValidation()
            .ValidateOnStart();

        services.AddScoped<IOutboxService, OutboxService<TDbContext>>();
        services.AddScoped<OutboxInsertInterceptor>();
        services.AddSingleton<IOutboxSignal, OutboxSignal>();

        return services;
    }
}