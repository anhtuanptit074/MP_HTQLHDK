using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Htqlhdk.Worker;

public sealed partial class BootstrapWorker(ILogger<BootstrapWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogBootstrapReady(logger);

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "HTQLHDK worker bootstrap is ready; no business job is enabled in stage 10")]
    private static partial void LogBootstrapReady(ILogger logger);
}
