using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BuildingBlocks.Outbox.Abstractions;

namespace BuildingBlocks.Outbox.EntityFrameworkCore;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public virtual void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "messaging");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Type).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.PartitionKey).HasMaxLength(200);
        builder.Property(x => x.OccurredOnUtc).IsRequired();
        builder.Property(x => x.RetryCount).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.Status).IsRequired().HasDefaultValue(OutboxMessageStatus.Pending);
        builder.Property(x => x.NextAttemptAtUtc).IsRequired(false);

        builder.HasIndex(x => new { x.PartitionKey, x.OccurredOnUtc })
            .HasFilter("\"status\" = 0")
            .HasDatabaseName("ix_outbox_partition_pending");
    }
}