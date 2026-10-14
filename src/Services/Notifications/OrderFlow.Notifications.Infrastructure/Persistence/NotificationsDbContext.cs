using Microsoft.EntityFrameworkCore;
using OrderFlow.BuildingBlocks.Infrastructure.Messaging;

namespace OrderFlow.Notifications.Infrastructure.Persistence;

internal sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.AddInboxMessages();
}
