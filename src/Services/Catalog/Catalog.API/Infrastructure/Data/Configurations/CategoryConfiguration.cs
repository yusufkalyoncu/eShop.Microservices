using Catalog.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.API.Infrastructure.Data.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.ComplexProperty(c => c.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.Value)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.ComplexProperty(c => c.Description, descBuilder =>
        {
            descBuilder.Property(d => d.Value)
                .HasColumnName("description")
                .HasMaxLength(500)
                .IsRequired();
        });
    }
}