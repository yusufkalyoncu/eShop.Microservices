using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.API.Domain.Models;

namespace Ordering.API.Infrastructure.Database.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.Id).ValueGeneratedNever();

        builder.Property(oi => oi.ProductId).IsRequired();
        builder.Property(oi => oi.ProductName).IsRequired().HasMaxLength(256);
        builder.Property(oi => oi.Price).HasColumnType("decimal(18,2)");
        builder.Property(oi => oi.Quantity).IsRequired();
    }
}