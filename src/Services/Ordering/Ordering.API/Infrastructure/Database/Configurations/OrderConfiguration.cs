using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.API.Domain.Models;

namespace Ordering.API.Infrastructure.Database.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.UserName).IsRequired().HasMaxLength(256);
        builder.Property(o => o.Status).IsRequired();
        builder.Property(o => o.TotalPrice).HasColumnType("decimal(18,2)");

        builder.ComplexProperty(o => o.ShippingAddress, a =>
        {
            a.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            a.Property(p => p.LastName).HasMaxLength(100).IsRequired();
            a.Property(p => p.EmailAddress).HasMaxLength(100);
            a.Property(p => p.AddressLine).HasMaxLength(200).IsRequired();
            a.Property(p => p.Country).HasMaxLength(100).IsRequired();
            a.Property(p => p.State).HasMaxLength(100).IsRequired();
            a.Property(p => p.ZipCode).HasMaxLength(50).IsRequired();
        });

        builder.ComplexProperty(o => o.BillingAddress, a =>
        {
            a.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            a.Property(p => p.LastName).HasMaxLength(100).IsRequired();
            a.Property(p => p.EmailAddress).HasMaxLength(100);
            a.Property(p => p.AddressLine).HasMaxLength(200).IsRequired();
            a.Property(p => p.Country).HasMaxLength(100).IsRequired();
            a.Property(p => p.State).HasMaxLength(100).IsRequired();
            a.Property(p => p.ZipCode).HasMaxLength(50).IsRequired();
        });

        builder.ComplexProperty(o => o.Payment, p =>
        {
            p.Property(x => x.CardName).HasMaxLength(100);
            p.Property(x => x.CardNumber).HasMaxLength(25);
            p.Property(x => x.Expiration).HasMaxLength(10);
            p.Property(x => x.CVV).HasMaxLength(5);
            p.Property(x => x.PaymentMethod);
        });

        builder.HasMany(o => o.OrderItems)
            .WithOne()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}