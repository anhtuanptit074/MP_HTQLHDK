$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$uiDir = Join-Path $root 'ui_design'
$failures = [System.Collections.Generic.List[string]]::new()

function Assert-True([bool]$Condition, [string]$Message) {
    if (-not $Condition) { $script:failures.Add($Message) }
}

$requiredFiles = @(
    'README.md',
    '01_sitemap_va_user_flow.md',
    '02_danh_muc_man_hinh_va_wireframe.md',
    '03_design_system.md',
    '04_ma_tran_hien_thi_masking.md',
    '05_accessibility_responsive.md',
    '06_ma_tran_truy_vet_ui_api.md',
    '07_usability_va_nghiem_thu.md'
)
foreach ($name in $requiredFiles) { Assert-True (Test-Path -LiteralPath (Join-Path $uiDir $name)) "Missing $name" }

$all = ($requiredFiles | ForEach-Object { Get-Content -LiteralPath (Join-Path $uiDir $_) -Raw -Encoding UTF8 }) -join "`n"
foreach ($id in 1..32) {
    $uiId = 'UI-{0:D2}' -f $id
    Assert-True ($all -match [regex]::Escape($uiId)) "Missing screen $uiId"
}

$api = Get-Content -LiteralPath (Join-Path $root 'api_design\01_openapi.yaml') -Encoding UTF8
$operationIds = @($api | Select-String '^      operationId: ([A-Za-z0-9]+)$' | ForEach-Object { $_.Matches[0].Groups[1].Value })
$trace = Get-Content -LiteralPath (Join-Path $uiDir '06_ma_tran_truy_vet_ui_api.md') -Raw -Encoding UTF8
foreach ($operationId in $operationIds) {
    Assert-True ($trace -match "(?<![A-Za-z0-9])$([regex]::Escape($operationId))(?![A-Za-z0-9])") "Missing API mapping $operationId"
}
Assert-True ($operationIds.Count -eq 57) "Expected 57 API operations, found $($operationIds.Count)"

$markdownFiles = Get-ChildItem -LiteralPath $uiDir -Filter '*.md' -File
foreach ($file in $markdownFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
    foreach ($match in [regex]::Matches($content, '\]\(([^)#]+)(?:#[^)]+)?\)')) {
        $target = $match.Groups[1].Value
        if ($target -match '^(https?:|mailto:)') { continue }
        $resolved = Join-Path $file.DirectoryName $target
        Assert-True (Test-Path -LiteralPath $resolved) "Broken link in $($file.Name): $target"
    }
}

foreach ($term in @('Source Risk','Source Confidence','Final Severity','Partially Completed','Idempotency-Key','If-Match','WCAG 2.2 AA','object key','token hash')) {
    Assert-True ($all -match [regex]::Escape($term)) "Required design concern missing: $term"
}

$forbiddenActions = Select-String -Path (Join-Path $uiDir '*.md') -Pattern 'nút.+(chạy|khởi động|dừng).+(ZAP|scanner)' -CaseSensitive:$false
$positiveForbidden = @($forbiddenActions | Where-Object { $_.Line -notmatch '(không|Không|không có|Không có)' })
Assert-True ($positiveForbidden.Count -eq 0) 'UI appears to define a scanner-control action.'

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Output 'PASS: 8 required UI-design files present; 32 screen IDs covered.'
Write-Output "PASS: $($operationIds.Count) API operations mapped; local Markdown links resolved."
Write-Output 'PASS: security, concurrency, masking, Partial import and accessibility concerns present.'
