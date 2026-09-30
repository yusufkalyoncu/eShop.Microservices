using Microsoft.EntityFrameworkCore;
using Ordering.Saga.StateMachines;

namespace Ordering.Saga.Infrastructure;

public sealed class SagaDbContext(DbContextOptions<SagaDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<OrderState>(builder =>
        {
            builder.ToTable("order_states");
            
            builder.HasKey(x => x.CorrelationId);
            
            builder.Property(x => x.CurrentState).HasMaxLength(50).IsRequired();
            builder.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.PaymentToken).HasMaxLength(255);
            builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Version).IsConcurrencyToken();
            
            var dictionaryComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<Dictionary<Guid, int>>(
                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.GetHashCode())),
                c => c.ToDictionary(k => k.Key, v => v.Value));
            
            builder.Property(x => x.Items)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<Guid, int>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<Guid, int>()
                )
                .Metadata.SetValueComparer(dictionaryComparer);
        });
    }
}