using System.Reflection;
using BuildingBlocks.Outbox.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Payment.API.Domain.Entities;

namespace Payment.API.Infrastructure.Data;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.ApplyOutboxConfiguration();
    }
}