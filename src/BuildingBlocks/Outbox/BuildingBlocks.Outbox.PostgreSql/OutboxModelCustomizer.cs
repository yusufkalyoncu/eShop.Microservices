using BuildingBlocks.Outbox.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BuildingBlocks.Outbox.PostgreSql;

public class OutboxModelCustomizer(ModelCustomizerDependencies dependencies) 
    : ModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("outbox_messages"); 
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.Content).HasColumnType("jsonb").IsRequired();
            builder.Property(x => x.OccurredOnUtc).IsRequired();
            builder.Property(x => x.RetryCount).IsRequired().HasDefaultValue(0);

            builder.HasIndex(x => x.OccurredOnUtc)
                .HasFilter("\"processed_on_utc\" IS NULL");
        });
    }
}