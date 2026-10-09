using System.ComponentModel.DataAnnotations;

namespace Htqlhdk.Worker.Automation;

public sealed class ZapAutomationOptions
{
    public const string SectionName = "Htqlhdk:ZapAutomation";

    public bool Enabled { get; init; }

    [Required]
    public string DatabaseConnectionStringEnvironmentVariable { get; init; } = "HTQLHDK_DATABASE_URL";

    [Required]
    [RegularExpression(@"^.+@sha256:[0-9a-f]{64}$")]
    public string ImageReference { get; init; } =
        "ghcr.io/zaproxy/zaproxy:2.17.0@sha256:781a2bdaea47324e7bab583e2263f21d257b0aee61ed51521a5be45f5f5081ef";

    [Required]
    public string DockerExecutable { get; init; } = "docker";

    [Required]
    public string DockerNetwork { get; init; } = "bridge";

    [Required]
    public string WorkRoot { get; init; } = "local-data/zap-work";

    [Required]
    public string ArtifactRoot { get; init; } = "local-data/zap-artifacts";

    [Required]
    public string ArtifactKeyReference { get; init; } = "local-development-only";

    [Range(1, 300)]
    public int PollIntervalSeconds { get; init; } = 10;

    [Range(30, 3600)]
    public int LeaseSeconds { get; init; } = 120;

    [Range(5, 300)]
    public int HeartbeatSeconds { get; init; } = 30;

    [Range(1, 100)]
    public int DispatchBatchSize { get; init; } = 5;

    [Required]
    [RegularExpression(@"^[1-9][0-9]*(?:[kKmMgG])?$")]
    public string MemoryLimit { get; init; } = "2g";

    [Required]
    [RegularExpression(@"^[0-9]+(?:\.[0-9]+)?$")]
    public string CpuLimit { get; init; } = "2";

    [Range(1024, 524_288_000)]
    public long MaxReportBytes { get; init; } = 52_428_800;

    public bool CleanupWorkDirectory { get; init; } = true;
}
