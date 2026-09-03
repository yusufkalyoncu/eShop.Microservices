using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BuildingBlocks.Inbox.PostgreSql;

public static class InboxDbContextOptionsExtensions
{
    public static DbContextOptionsBuilder UsePostgreSqlInbox(this DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ReplaceService<IModelCustomizer, InboxModelCustomizer>();
        optionsBuilder.UseExceptionProcessor();
        return optionsBuilder;
    }
}