using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Htqlhdk.Worker.Automation;

public sealed partial class ZapAutomationOrchestrator(
    ZapExecutionRepository repository,
    ZapPlanRenderer planRenderer,
    IZapRunner runner,
    ZapReportInspector reportInspector,
    IZapArtifactStore artifactStore,
    IOptions<ZapAutomationOptions> options,
    ILogger<ZapAutomationOrchestrator> logger)
{
    private readonly ZapAutomationOptions options = options.Value;

    public Task<int> RecoverExpiredLeasesAsync(CancellationToken cancellationToken) =>
        repository.RecoverExpiredLeasesAsync(options.DispatchBatchSize, cancellationToken);

    public Task<int> DispatchDueSchedulesAsync(CancellationToken cancellationToken) =>
        repository.DispatchDueSchedulesAsync(options.DispatchBatchSize, options.ImageReference, cancellationToken);

    public async Task<bool> ProcessNextAsync(string workerId, CancellationToken cancellationToken)
    {
        TimeSpan lease = TimeSpan.FromSeconds(options.LeaseSeconds);
        ZapJobSpecification? job = await repository.ClaimNextAsync(workerId, lease, cancellationToken);
        if (job is null)
        {
            return false;
        }

        string? workDirectory = null;
        try
        {
            string plan = planRenderer.Render(job);
            workDirectory = Path.GetFullPath(Path.Combine(options.WorkRoot, job.ExecutionCode));
            await repository.SetStatusAsync(job.ExecutionId, workerId, "Starting Runner", cancellationToken);

            using CancellationTokenSource jobCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Task<ZapRunResult> runTask = runner.RunAsync(job, plan, jobCancellation.Token);
            await repository.SetStatusAsync(job.ExecutionId, workerId, "Running", cancellationToken);

            while (!runTask.IsCompleted)
            {
                Task delay = Task.Delay(TimeSpan.FromSeconds(options.HeartbeatSeconds), cancellationToken);
                Task completed = await Task.WhenAny(runTask, delay);
                if (completed == runTask)
                {
                    break;
                }

                bool cancelRequested = await repository.RenewLeaseAsync(
                    job.ExecutionId, workerId, lease, cancellationToken);
                if (cancelRequested)
                {
                    await jobCancellation.CancelAsync();
                }
            }

            ZapRunResult run = await runTask;
            bool cancelRequestedAfterRun = await repository.RenewLeaseAsync(
                job.ExecutionId, workerId, lease, cancellationToken);
            if (cancelRequestedAfterRun)
            {
                await repository.FailAsync(
                    job, workerId, "Cancelled", "ZAP_CANCELLED",
                    "ZAP execution was cancelled by an approved request.", cancellationToken);
                return true;
            }

            if (run.TimedOut)
            {
                await repository.FailAsync(
                    job, workerId, "Timed Out", "ZAP_TIMEOUT", "ZAP runner exceeded its approved duration.",
                    cancellationToken);
                return true;
            }

            if (run.ExitCode is not (0 or 2))
            {
                await repository.FailAsync(
                    job, workerId, "Failed", "ZAP_RUNNER_FAILED", "ZAP runner ended without a usable report.",
                    cancellationToken);
                return true;
            }

            await repository.SetStatusAsync(job.ExecutionId, workerId, "Collecting Report", cancellationToken);
            ZapReportMetadata report = await reportInspector.InspectAsync(
                run.JsonReportPath, job.TargetUrls, options.MaxReportBytes, cancellationToken);
            StoredZapArtifact json = await artifactStore.StoreAsync(
                job.ExecutionCode, run.JsonReportPath, "Traditional JSON", "application/json", cancellationToken);
            StoredZapArtifact? html = null;
            if (File.Exists(run.HtmlReportPath) && new FileInfo(run.HtmlReportPath).Length > 0)
            {
                html = await artifactStore.StoreAsync(
                    job.ExecutionCode, run.HtmlReportPath, "Traditional HTML", "text/html", cancellationToken);
            }

            await repository.CompleteAsync(
                job, workerId, report, json, html, run.ExitCode, options.ArtifactKeyReference,
                cancellationToken);
            LogReportReceived(logger, job.ExecutionId, report.SiteCount, report.AlertCount);
            return true;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            await repository.FailAsync(
                job, workerId, "Cancelled", "ZAP_CANCELLED", "ZAP execution was cancelled by an approved request.",
                CancellationToken.None);
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogExecutionFailure(logger, job.ExecutionId, exception.GetType().Name);
            try
            {
                await repository.FailAsync(
                    job, workerId, "Failed", "ZAP_AUTOMATION_FAILED",
                    "ZAP automation failed; inspect protected operational telemetry.", CancellationToken.None);
            }
            catch (Exception persistenceException)
            {
                if (logger.IsEnabled(LogLevel.Critical))
                {
                    string persistenceErrorType = persistenceException.GetType().Name;
                    LogFailurePersistenceError(logger, job.ExecutionId, persistenceErrorType);
                }
            }

            return true;
        }
        finally
        {
            if (options.CleanupWorkDirectory && workDirectory is not null)
            {
                TryDeleteWorkDirectory(workDirectory);
            }
        }
    }

    private void TryDeleteWorkDirectory(string workDirectory)
    {
        string root = Path.GetFullPath(options.WorkRoot);
        string resolved = Path.GetFullPath(workDirectory);
        if (!resolved.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            if (Directory.Exists(resolved))
            {
                Directory.Delete(resolved, recursive: true);
            }
        }
        catch (IOException)
        {
            LogCleanupFailure(logger);
        }
        catch (UnauthorizedAccessException)
        {
            LogCleanupFailure(logger);
        }
    }

    [LoggerMessage(EventId = 1101, Level = LogLevel.Information,
        Message = "ZAP execution {ExecutionId} report received with {SiteCount} sites and {AlertCount} alert groups")]
    private static partial void LogReportReceived(ILogger logger, long executionId, int siteCount, int alertCount);

    [LoggerMessage(EventId = 1102, Level = LogLevel.Error,
        Message = "ZAP execution {ExecutionId} failed with error type {ErrorType}")]
    private static partial void LogExecutionFailure(ILogger logger, long executionId, string errorType);

    [LoggerMessage(EventId = 1103, Level = LogLevel.Critical,
        Message = "Could not persist failure state for ZAP execution {ExecutionId}; error type {ErrorType}")]
    private static partial void LogFailurePersistenceError(ILogger logger, long executionId, string errorType);

    [LoggerMessage(EventId = 1104, Level = LogLevel.Warning,
        Message = "A ZAP temporary work directory could not be removed")]
    private static partial void LogCleanupFailure(ILogger logger);
}
