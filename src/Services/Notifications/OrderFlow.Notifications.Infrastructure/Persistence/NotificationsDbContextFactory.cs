using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Notifications.Infrastructure.Persistence;

internal sealed class NotificationsDbContextFactory
    : IDesignTimeDbContextFactory<NotificationsDbContext>
{
    public NotificationsDbContext CreateDbContext(string[] args) =>
        new(
            new DbContextOptionsBuilder<NotificationsDbContext>()
                .UseNpgsql("Host=localhost;Database=notifications_db")
                .UseSnakeCaseNamingConvention()
                .Options
        );
}
