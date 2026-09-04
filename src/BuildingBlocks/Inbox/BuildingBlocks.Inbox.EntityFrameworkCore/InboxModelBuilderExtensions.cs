using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Inbox.EntityFrameworkCore;

public static class InboxModelBuilderExtensions
{
    public static void ApplyInboxConfiguration(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new InboxMessageConfiguration());
    }
}