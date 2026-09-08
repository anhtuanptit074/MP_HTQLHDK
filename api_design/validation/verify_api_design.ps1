$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$apiDir = Join-Path $root 'api_design'
$openApiPath = Join-Path $apiDir '01_openapi.yaml'
$lines = Get-Content -LiteralPath $openApiPath -Encoding UTF8
$failures = [System.Collections.Generic.List[string]]::new()

function Assert-True([bool]$Condition, [string]$Message) {
    if (-not $Condition) { $script:failures.Add($Message) }
}

Assert-True ($lines[0] -eq 'openapi: 3.1.0') 'OpenAPI version must be 3.1.0.'
Assert-True (($lines | Select-String '^servers:$').Count -eq 1) 'servers section missing or duplicated.'
Assert-True (($lines | Select-String '^  - url: /api/v1$').Count -eq 1) 'API /api/v1 server missing.'
Assert-True (($lines | Select-String '^components:$').Count -eq 1) 'components section missing or duplicated.'

$operationLines = $lines | Select-String '^      operationId: ([A-Za-z0-9]+)$'
$contractLines = $lines | Select-String '^      x-contract:$'
$useCaseLines = $lines | Select-String '^      x-use-cases:'
$operationIds = @($operationLines | ForEach-Object { $_.Matches[0].Groups[1].Value })
Assert-True ($operationIds.Count -eq 57) "Expected 57 operations, found $($operationIds.Count)."
Assert-True (($operationIds | Sort-Object -Unique).Count -eq $operationIds.Count) 'operationId values must be unique.'
Assert-True ($contractLines.Count -eq $operationIds.Count) 'Every operation must have x-contract.'
Assert-True ($useCaseLines.Count -eq $operationIds.Count) 'Every operation must have x-use-cases.'

$requiredContractKeys = @('purpose','actors','scope','state','transaction','concurrency','audit','tests')
foreach ($op in $operationLines) {
    $start = $op.LineNumber - 1
    $next = $operationLines | Where-Object LineNumber -gt $op.LineNumber | Select-Object -First 1
    $end = if ($next) { $next.LineNumber - 2 } else { ($lines | Select-String '^components:$').LineNumber - 2 }
    $block = $lines[$start..$end]
    foreach ($key in $requiredContractKeys) {
        Assert-True (($block | Select-String "^        $key\s*:").Count -eq 1) "$($op.Matches[0].Groups[1].Value) missing contract key $key."
    }
}

$componentLine = ($lines | Select-String '^components:$').LineNumber
$pathsAfterComponents = $lines[$componentLine..($lines.Count - 1)] | Select-String '^  /'
Assert-True ($pathsAfterComponents.Count -eq 0) 'Path found after components section.'

$refs = @($lines | Select-String "#/components/(parameters|responses|schemas|headers)/([A-Za-z0-9]+)" -AllMatches | ForEach-Object Matches | ForEach-Object Value | Sort-Object -Unique)
$definitions = [System.Collections.Generic.HashSet[string]]::new()
$section = $null
foreach ($line in $lines) {
    if ($line -match '^  (parameters|responses|schemas|headers):$') { $section = $Matches[1]; continue }
    if ($section -and $line -match '^    ([A-Za-z0-9]+):') { [void]$definitions.Add("#/components/$section/$($Matches[1])") }
    if ($line -match '^  [A-Za-z].*:$' -and $line -notmatch '^  (parameters|responses|schemas|headers):$') { $section = $null }
}
foreach ($ref in $refs) { Assert-True ($definitions.Contains($ref)) "Missing component definition $ref." }

$markdownFiles = Get-ChildItem -LiteralPath $apiDir -Filter '*.md' -File
foreach ($file in $markdownFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
    foreach ($match in [regex]::Matches($content, '\]\(([^)#]+)(?:#[^)]+)?\)')) {
        $target = $match.Groups[1].Value
        if ($target -match '^(https?:|mailto:)') { continue }
        $resolved = Join-Path $file.DirectoryName $target
        Assert-True (Test-Path -LiteralPath $resolved) "Broken link in $($file.Name): $target"
    }
}

$documented = Get-Content -LiteralPath (Join-Path $apiDir '06_truy_vet_va_nghiem_thu.md') -Raw -Encoding UTF8
$ucFiles = Get-ChildItem -LiteralPath (Join-Path $root 'tai_lieu_use_case') -Filter 'UC-*.md' -File
foreach ($ucFile in $ucFiles) {
    $code = [regex]::Match($ucFile.Name, '^UC-([A-Z]+\d+)').Groups[1].Value
    Assert-True ($documented -match "(?m)^\| $([regex]::Escape($code)) \|") "Use case $code is not mapped."
}

$forbiddenPath = $lines | Select-String '^  /.*(zap|scanner).*(start|stop|run|control)'
Assert-True ($forbiddenPath.Count -eq 0) 'Scanner-control endpoint is outside scope.'

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Output "PASS: 57 unique operations; 57 contracts; 57 use-case annotations."
Write-Output "PASS: $($refs.Count) component references resolved; $($ucFiles.Count) use cases mapped."
Write-Output "PASS: local Markdown links resolved; no scanner-control path found."
