using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Payments.Infrastructure.Persistence;

internal sealed class PaymentsDbContextFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] args) =>
        new(
            new DbContextOptionsBuilder<PaymentsDbContext>()
                .UseNpgsql("Host=localhost;Database=payments_db")
                .UseSnakeCaseNamingConvention()
                .Options
        );
}
