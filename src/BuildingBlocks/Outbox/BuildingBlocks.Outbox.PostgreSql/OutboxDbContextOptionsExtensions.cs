using BuildingBlocks.Outbox.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Outbox.PostgreSql;

public static class OutboxDbContextOptionsExtensions
{
    public static DbContextOptionsBuilder UsePostgreSqlOutbox(
        this DbContextOptionsBuilder optionsBuilder,
        IServiceProvider serviceProvider)
    {
        var interceptor = serviceProvider.GetRequiredService<OutboxInsertInterceptor>();
        optionsBuilder.AddInterceptors(interceptor);
        optionsBuilder.ReplaceService<IModelCustomizer, OutboxModelCustomizer>();

        return optionsBuilder;
    }
}