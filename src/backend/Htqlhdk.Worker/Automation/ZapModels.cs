namespace Htqlhdk.Worker.Automation;

public enum ZapSpiderKind
{
    Traditional,
    Client
}

public enum ZapAuthenticationKind
{
    None,
    Header
}

public sealed record ZapScopeRule(
    string Effect,
    string TargetType,
    string TargetValue,
    string MatchMode);

public sealed record ZapJobSpecification(
    long ExecutionId,
    string ExecutionCode,
    long ScanId,
    long AssetId,
    long ServiceUserId,
    string[] TargetUrls,
    ZapScopeRule[] ScopeRules,
    ZapSpiderKind SpiderKind,
    bool ActiveScanEnabled,
    ZapAuthenticationKind AuthenticationKind,
    string? AuthenticationHeaderName,
    string? AuthenticationSecretReference,
    int SpiderMaxMinutes,
    int PassiveWaitMaxMinutes,
    int ActiveScanMaxMinutes,
    int MaxRuleMinutes,
    int RequestDelayMilliseconds,
    int ThreadPerHost,
    DateTimeOffset StartedAt,
    int PlannedDurationMinutes);

public sealed record ZapRunResult(
    int ExitCode,
    bool TimedOut,
    string JsonReportPath,
    string HtmlReportPath);

public sealed record ZapReportMetadata(
    string ZapVersion,
    long SizeBytes,
    string Sha256,
    int SiteCount,
    int AlertCount);

public sealed record StoredZapArtifact(
    string ObjectKey,
    string Sha256,
    long SizeBytes,
    string MediaType,
    string ArtifactKind,
    string OriginalName);
