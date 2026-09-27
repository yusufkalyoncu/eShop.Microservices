using BuildingBlocks.Inbox.EntityFrameworkCore;
using BuildingBlocks.Outbox.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Domain.Models;

namespace Ordering.API.Infrastructure.Database;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly);
        modelBuilder.ApplyOutboxConfiguration();
        modelBuilder.ApplyInboxConfiguration();
        
        base.OnModelCreating(modelBuilder);
    }
}