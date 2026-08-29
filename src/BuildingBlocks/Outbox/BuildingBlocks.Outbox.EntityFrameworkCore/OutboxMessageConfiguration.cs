using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Outbox.EntityFrameworkCore;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public virtual void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Type).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.OccurredOnUtc).IsRequired();
        builder.Property(x => x.RetryCount).IsRequired().HasDefaultValue(0);
    }
}