$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$failures = [System.Collections.Generic.List[string]]::new()

function Assert-True([bool]$Condition, [string]$Message) {
    if (-not $Condition) { $script:failures.Add($Message) }
}

$requiredFiles = @(
    '.editorconfig', '.gitattributes', '.gitignore', '.env.local.example',
    'global.json', 'Directory.Build.props', 'NuGet.Config', 'HTQLHDK.slnx', 'compose.yaml',
    'src/backend/Htqlhdk.Api/Htqlhdk.Api.csproj',
    'src/backend/Htqlhdk.Worker/Htqlhdk.Worker.csproj',
    'src/frontend/package.json', 'src/frontend/package-lock.json',
    'ops/migrations/V001__baseline.psql', 'ops/migrations/V001__manifest.json',
    'ops/migrations/verify_baseline.psql', 'ops/migrations/verify_migration.mjs',
    'tests/fixtures/sql/load_test_fixture.psql',
    '.github/workflows/ci.yml', '.github/workflows/dependency-review.yml',
    '.github/dependabot.yml', '.github/pull_request_template.md',
    'docs/development/01_local_setup.md',
    'docs/development/02_coding_standards.md',
    'docs/development/03_configuration_and_secrets.md',
    'docs/development/04_migrations.md',
    'docs/development/05_definition_of_done.md',
    'docs/development/06_branch_and_review.md',
    'docs/development/07_environment_matrix.md',
    'docs/development/08_verification_report.md'
)

foreach ($relativePath in $requiredFiles) {
    Assert-True (Test-Path -LiteralPath (Join-Path $root $relativePath)) "Missing $relativePath"
}

$globalJson = Get-Content -Raw -LiteralPath (Join-Path $root 'global.json') | ConvertFrom-Json
Assert-True ($globalJson.sdk.version -eq '10.0.400') 'global.json must pin .NET SDK 10.0.400.'
Assert-True (-not $globalJson.sdk.allowPrerelease) 'Prerelease .NET SDK must be disabled.'

$packageJson = Get-Content -Raw -LiteralPath (Join-Path $root 'src/frontend/package.json') | ConvertFrom-Json
$allVersions = @($packageJson.dependencies.PSObject.Properties.Value) + @($packageJson.devDependencies.PSObject.Properties.Value)
foreach ($version in $allVersions) {
    Assert-True ($version -match '^\d+\.\d+\.\d+$') "Frontend dependency is not exact: $version"
}

$manifest = Get-Content -Raw -LiteralPath (Join-Path $root 'ops/migrations/V001__manifest.json') | ConvertFrom-Json
foreach ($file in $manifest.files) {
    $fullPath = Join-Path $root $file.path
    Assert-True (Test-Path -LiteralPath $fullPath) "Migration source missing: $($file.path)"
    if (Test-Path -LiteralPath $fullPath) {
        $actual = (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).Hash.ToLowerInvariant()
        Assert-True ($actual -eq $file.sha256) "Migration checksum drift: $($file.path)"
    }
}

$textExtensions = @('.cs', '.csproj', '.json', '.js', '.ts', '.tsx', '.yml', '.yaml', '.md', '.ps1', '.psql', '.sql', '.env', '.example')
$scanFiles = Get-ChildItem -LiteralPath $root -Recurse -File | Where-Object {
    $_.FullName -notmatch '[\\/](\.git|node_modules|bin|obj|dist|coverage|\.codex_tmp|\.docx_review)[\\/]' -and
    $textExtensions -contains $_.Extension.ToLowerInvariant()
}
$secretPatterns = @(
    'AKIA[0-9A-Z]{16}',
    'gh[pousr]_[A-Za-z0-9]{30,}',
    '-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----',
    '(?i)Bearer\s+[A-Za-z0-9._-]{24,}'
)
foreach ($file in $scanFiles) {
    $content = Get-Content -Raw -LiteralPath $file.FullName -ErrorAction SilentlyContinue
    foreach ($pattern in $secretPatterns) {
        Assert-True (-not ($content -match $pattern)) "Potential secret in $($file.FullName): pattern $pattern"
    }
}

$markdownFiles = @(
    Get-Item -LiteralPath (Join-Path $root 'README.md'), (Join-Path $root 'quy_trinh_xay_dung_he_thong.md'), (Join-Path $root 'SECURITY.md')
) + @(Get-ChildItem -LiteralPath (Join-Path $root 'docs/development') -Filter '*.md' -File)
foreach ($file in $markdownFiles) {
    $content = Get-Content -Raw -LiteralPath $file.FullName -Encoding UTF8
    foreach ($match in [regex]::Matches($content, '\]\(([^)#]+)(?:#[^)]+)?\)')) {
        $target = $match.Groups[1].Value
        if ($target -match '^(https?:|mailto:)') { continue }
        $resolved = Join-Path $file.DirectoryName $target
        Assert-True (Test-Path -LiteralPath $resolved) "Broken local link in $($file.FullName): $target"
    }
}

$unsafeReports = @(Get-ChildItem -LiteralPath $root -Recurse -File -Include '*.har', '*.zap', '*.session' | Where-Object {
    $_.FullName -notmatch '[\\/]tests[\\/]fixtures[\\/]zap[\\/]'
})
Assert-True ($unsafeReports.Count -eq 0) 'Raw scanner/session file found outside sanitized test fixtures.'

$compose = Get-Content -Raw -LiteralPath (Join-Path $root 'compose.yaml')
Assert-True ($compose -match 'postgres:17\.11-alpine3\.24@sha256:[a-f0-9]{64}') 'PostgreSQL image must be tag-and-digest pinned.'
Assert-True ($compose -notmatch '(?im)^\s*POSTGRES_PASSWORD:\s*(?!\$\{)\S') 'Compose must not contain a literal database password.'

& pwsh -NoProfile -File (Join-Path $root 'api_design/validation/verify_api_design.ps1')
if ($LASTEXITCODE -ne 0) { $failures.Add('API design validation failed.') }
& pwsh -NoProfile -File (Join-Path $root 'ui_design/validation/verify_ui_design.ps1')
if ($LASTEXITCODE -ne 0) { $failures.Add('UI design validation failed.') }
$pgliteRuntime = Join-Path $root 'src/frontend/node_modules/@electric-sql/pglite/dist/index.js'
Assert-True (Test-Path -LiteralPath $pgliteRuntime) 'PGlite runtime missing; run npm ci in src/frontend first.'
if (Test-Path -LiteralPath $pgliteRuntime) {
    & node (Join-Path $root 'database/validation/verify_database.mjs') $pgliteRuntime
    if ($LASTEXITCODE -ne 0) { $failures.Add('Database artifact validation failed.') }
    & node (Join-Path $root 'ops/migrations/verify_migration.mjs') $pgliteRuntime
    if ($LASTEXITCODE -ne 0) { $failures.Add('Migration repeatability validation failed.') }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Output "PASS: $($requiredFiles.Count) stage-10 deliverables present."
Write-Output 'PASS: toolchains and frontend dependencies are version-pinned; migration checksums match.'
Write-Output 'PASS: high-confidence secret patterns and unsafe scanner artifacts not found.'
