using Catalog.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Infrastructure.Data;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}