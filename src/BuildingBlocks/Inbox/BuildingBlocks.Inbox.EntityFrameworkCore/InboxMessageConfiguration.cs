using BuildingBlocks.Inbox.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Inbox.EntityFrameworkCore;

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages", "messaging");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Type).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Content).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.PartitionKey).HasMaxLength(200);
        builder.Property(x => x.OccurredOnUtc).IsRequired();
        builder.Property(x => x.ReceivedOnUtc).IsRequired();
        builder.Property(x => x.RetryCount).IsRequired().HasDefaultValue(0);

        builder.HasIndex(x => x.OccurredOnUtc).HasFilter("\"status\" = 0");
        builder.HasIndex(x => new { x.PartitionKey, x.OccurredOnUtc }).HasFilter("\"status\" = 0");
        builder.HasIndex(x => x.ProcessedOnUtc).HasFilter("\"status\" = 1");
    }
}