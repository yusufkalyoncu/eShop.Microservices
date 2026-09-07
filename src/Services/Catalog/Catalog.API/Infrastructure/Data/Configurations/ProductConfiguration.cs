using Catalog.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.API.Infrastructure.Data.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.ComplexProperty(p => p.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.Value)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.ComplexProperty(p => p.Description, descBuilder =>
        {
            descBuilder.Property(d => d.Value)
                .HasColumnName("description")
                .HasMaxLength(500)
                .IsRequired();
        });

        builder.ComplexProperty(p => p.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("price_amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("price_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .IsRequired();
    }
}