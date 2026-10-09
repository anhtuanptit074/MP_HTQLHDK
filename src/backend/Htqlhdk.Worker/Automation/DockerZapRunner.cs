using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace Htqlhdk.Worker.Automation;

public interface IZapRunner
{
    Task<ZapRunResult> RunAsync(
        ZapJobSpecification job,
        string planYaml,
        CancellationToken cancellationToken);
}

public sealed class DockerZapRunner(
    IOptions<ZapAutomationOptions> options,
    IZapSecretProvider secretProvider) : IZapRunner
{
    private readonly ZapAutomationOptions options = options.Value;

    public async Task<ZapRunResult> RunAsync(
        ZapJobSpecification job,
        string planYaml,
        CancellationToken cancellationToken)
    {
        string workRoot = Path.GetFullPath(options.WorkRoot);
        string workDirectory = Path.GetFullPath(Path.Combine(workRoot, job.ExecutionCode));
        if (!workDirectory.StartsWith(workRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("ZAP work directory escaped the configured root.");
        }

        string outputDirectory = Path.Combine(workDirectory, "output");
        Directory.CreateDirectory(outputDirectory);
        string planPath = Path.Combine(workDirectory, "zap-plan.yaml");
        await File.WriteAllTextAsync(planPath, planYaml, cancellationToken);

        string containerName = $"htqlhdk-zap-{job.ExecutionCode.ToLowerInvariant()}";
        ProcessStartInfo startInfo = CreateStartInfo(workDirectory, containerName);
        await AddAuthenticationAsync(startInfo, job, cancellationToken);

        using Process process = new() { StartInfo = startInfo };
        if (!process.Start())
        {
            throw new InvalidOperationException("Docker did not start the ZAP runner process.");
        }

        Task outputPump = DrainAsync(process.StandardOutput, cancellationToken);
        Task errorPump = DrainAsync(process.StandardError, cancellationToken);
        TimeSpan timeout = TimeSpan.FromMinutes(Math.Max(
            job.PlannedDurationMinutes,
            job.SpiderMaxMinutes + job.PassiveWaitMaxMinutes +
            (job.ActiveScanEnabled ? job.ActiveScanMaxMinutes + job.PassiveWaitMaxMinutes : 0) + 5));
        using CancellationTokenSource timeoutSource = new(timeout);
        using CancellationTokenSource runSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutSource.Token);

        bool timedOut = false;
        try
        {
            await process.WaitForExitAsync(runSource.Token);
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            timedOut = true;
            await StopContainerAsync(containerName);
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (OperationCanceledException)
        {
            await StopContainerAsync(containerName);
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }

            throw;
        }

        await Task.WhenAll(outputPump, errorPump);
        string jsonPath = Path.Combine(outputDirectory, "report.json");
        string htmlPath = Path.Combine(outputDirectory, "report.html");
        return new ZapRunResult(process.HasExited ? process.ExitCode : -1, timedOut, jsonPath, htmlPath);
    }

    private ProcessStartInfo CreateStartInfo(string workDirectory, string containerName)
    {
        ProcessStartInfo info = new()
        {
            FileName = options.DockerExecutable,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        string[] arguments =
        [
            "run", "--rm",
            "--name", containerName,
            "--network", options.DockerNetwork,
            "--memory", options.MemoryLimit,
            "--cpus", options.CpuLimit,
            "--security-opt", "no-new-privileges:true",
            "--mount", $"type=bind,source={workDirectory},target=/zap/wrk",
            options.ImageReference,
            "zap.sh", "-cmd", "-autorun", "/zap/wrk/zap-plan.yaml"
        ];
        foreach (string argument in arguments)
        {
            info.ArgumentList.Add(argument);
        }

        return info;
    }

    private async Task AddAuthenticationAsync(
        ProcessStartInfo info,
        ZapJobSpecification job,
        CancellationToken cancellationToken)
    {
        if (job.AuthenticationKind != ZapAuthenticationKind.Header)
        {
            return;
        }

        string secret = await secretProvider.GetSecretAsync(job.AuthenticationSecretReference!, cancellationToken);
        string site = new Uri(job.TargetUrls[0], UriKind.Absolute).GetLeftPart(UriPartial.Authority);
        AddContainerEnvironment(info, "ZAP_AUTH_HEADER", job.AuthenticationHeaderName!);
        AddContainerEnvironment(info, "ZAP_AUTH_HEADER_VALUE", secret);
        AddContainerEnvironment(info, "ZAP_AUTH_HEADER_SITE", site);
    }

    private static void AddContainerEnvironment(ProcessStartInfo info, string name, string value)
    {
        int imageIndex = info.ArgumentList.Count - 5;
        info.ArgumentList.Insert(imageIndex, "--env");
        info.ArgumentList.Insert(imageIndex + 1, name);
        info.Environment[name] = value;
    }

    private async Task StopContainerAsync(string containerName)
    {
        ProcessStartInfo stopInfo = new()
        {
            FileName = options.DockerExecutable,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        stopInfo.ArgumentList.Add("stop");
        stopInfo.ArgumentList.Add("--time");
        stopInfo.ArgumentList.Add("10");
        stopInfo.ArgumentList.Add(containerName);
        using Process stop = new() { StartInfo = stopInfo };
        if (stop.Start())
        {
            await Task.WhenAll(
                DrainAsync(stop.StandardOutput, CancellationToken.None),
                DrainAsync(stop.StandardError, CancellationToken.None),
                stop.WaitForExitAsync());
        }
    }

    public Task StopExecutionAsync(string code)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(code, "^[A-Za-z0-9_-]{1,40}$", System.Text.RegularExpressions.RegexOptions.CultureInvariant)) { throw new ArgumentException("Unsafe execution code.", nameof(code)); }
        return StopContainerAsync("htqlhdk-zap-" + code.ToLowerInvariant());
    }

    private static async Task DrainAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        while (await reader.ReadLineAsync(cancellationToken) is not null)
        {
            // ZAP output can contain target URLs or sensitive response fragments. Do not log it.
        }
    }
}
