using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using OrderFlow.Orders.Application.Orders;
using OrderFlow.Orders.Application.Orders.Saga;

namespace OrderFlow.Orders.Infrastructure.Fulfillment;

internal sealed partial class FulfillmentScheduler(
    IServiceScopeFactory scopeFactory,
    IOptions<FulfillmentOptions> options,
    TimeProvider timeProvider,
    ILogger<FulfillmentScheduler> logger
) : BackgroundService
{
    private const int BatchSize = 20;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMilliseconds(options.Value.PollIntervalMilliseconds);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunDueStepsAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (NpgsqlException exception)
            {
                LogPollFailed(exception);
            }

            await Task.Delay(interval, timeProvider, stoppingToken)
                .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    private async Task RunDueStepsAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Guid> due;
        var scope = scopeFactory.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            due = await scope
                .ServiceProvider.GetRequiredService<IOrderRepository>()
                .GetDueAsync(timeProvider.GetUtcNow(), BatchSize, cancellationToken)
                .ConfigureAwait(false);
        }

        foreach (var orderId in due)
        {
            var stepScope = scopeFactory.CreateAsyncScope();
            await using (stepScope.ConfigureAwait(false))
            {
                try
                {
                    await stepScope
                        .ServiceProvider.GetRequiredService<OrderFulfillment>()
                        .AdvanceAsync(orderId, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    LogConcurrentChange(orderId);
                }
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Fulfillment poll failed; retrying")]
    private partial void LogPollFailed(Exception exception);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Order {OrderId} changed concurrently; the step runs again on the next poll"
    )]
    private partial void LogConcurrentChange(Guid orderId);
}
