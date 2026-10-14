using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrderFlow.BuildingBlocks.ServiceDefaults.Shutdown;

internal sealed partial class ShutdownDrainService(
    IOptions<GracefulShutdownOptions> options,
    ILogger<ShutdownDrainService> logger
) : IHostedLifecycleService
{
    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromSeconds(options.Value.DrainDelaySeconds);
        if (delay == TimeSpan.Zero)
        {
            return;
        }

        LogDraining(delay.TotalSeconds);
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Shutdown requested; draining for {DelaySeconds} seconds"
    )]
    private partial void LogDraining(double delaySeconds);
}
