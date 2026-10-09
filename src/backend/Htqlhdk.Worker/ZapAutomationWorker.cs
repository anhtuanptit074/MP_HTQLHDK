using Htqlhdk.Worker.Automation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Htqlhdk.Worker;

public sealed partial class ZapAutomationWorker(
    IServiceProvider services,
    IOptions<ZapAutomationOptions> options,
    ILogger<ZapAutomationWorker> logger) : BackgroundService
{
    private readonly ZapAutomationOptions options = options.Value;
    private readonly string workerId = CreateWorkerId();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled)
        {
            LogDisabled(logger);
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            return;
        }

        ZapAutomationOrchestrator orchestrator = services.GetRequiredService<ZapAutomationOrchestrator>();
        LogStarted(logger);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                int recovered = await orchestrator.RecoverExpiredLeasesAsync(stoppingToken);
                if (recovered > 0)
                {
                    LogRecoveredExpiredLeases(logger, recovered);
                }

                int dispatched = await orchestrator.DispatchDueSchedulesAsync(stoppingToken);
                if (dispatched > 0)
                {
                    LogDispatched(logger, dispatched);
                }

                bool processed = await orchestrator.ProcessNextAsync(workerId, stoppingToken);
                if (!processed)
                {
                    await Task.Delay(TimeSpan.FromSeconds(options.PollIntervalSeconds), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogLoopFailure(logger, exception.GetType().Name);
                await Task.Delay(TimeSpan.FromSeconds(options.PollIntervalSeconds), stoppingToken);
            }
        }
    }

    [LoggerMessage(EventId = 1000, Level = LogLevel.Information,
        Message = "ZAP automation is disabled; worker is idle")]
    private static partial void LogDisabled(ILogger logger);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information,
        Message = "ZAP Automation Framework worker started")]
    private static partial void LogStarted(ILogger logger);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information,
        Message = "Dispatched {Count} due automated scan schedules")]
    private static partial void LogDispatched(ILogger logger, int count);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Error,
        Message = "ZAP worker loop failed with error type {ErrorType}")]
    private static partial void LogLoopFailure(ILogger logger, string errorType);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Warning,
        Message = "Marked {Count} ZAP executions as failed after their worker leases expired")]
    private static partial void LogRecoveredExpiredLeases(ILogger logger, int count);

    private static string CreateWorkerId()
    {
        string value = $"{Environment.MachineName}-{Environment.ProcessId}-{Guid.NewGuid():N}";
        return value[..Math.Min(120, value.Length)];
    }
}
