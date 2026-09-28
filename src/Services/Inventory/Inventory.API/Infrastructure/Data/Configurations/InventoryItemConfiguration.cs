using Inventory.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.API.Infrastructure.Data.Configurations;

internal sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("inventory_items");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.ProductId).IsUnique();

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.ComplexProperty(x => x.AvailableQuantity, quantityBuilder =>
        {
            quantityBuilder.Property(q => q.Value)
                .HasColumnName("available_quantity")
                .IsRequired();
        });
    }
}