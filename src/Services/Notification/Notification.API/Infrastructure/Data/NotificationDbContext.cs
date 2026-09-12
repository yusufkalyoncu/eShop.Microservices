using BuildingBlocks.Inbox.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Notification.API.Infrastructure.Data;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Only inbox_messages table — no domain entities
        modelBuilder.ApplyInboxConfiguration();
    }
}