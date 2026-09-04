using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Outbox.EntityFrameworkCore;

public static class OutboxModelBuilderExtensions
{
    public static void ApplyOutboxConfiguration(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new OutboxMessageConfiguration());
    }
}