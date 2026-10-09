param([int]$Port = 5002)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'use-local-tools.ps1')
$root = Split-Path -Parent $PSScriptRoot
$testRoot = Join-Path $root '.codex_tmp/core13-tests'
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
$settings = @{}
Get-Content -LiteralPath (Join-Path $root '.env.local') | ForEach-Object { if ($_ -match '^(HTQLHDK_[A-Z0-9_]+)=(.*)$') { $settings[$matches[1]] = $matches[2] } }
$settings['HTQLHDK_DB_NAME'] = 'htqlhdk_core_test_local'
$settings['HTQLHDK_SCHEMA_MODEL'] = 'core13'
$testEnv = Join-Path $testRoot '.env.local'
$settings.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" } | Set-Content -LiteralPath $testEnv -Encoding utf8
& (Join-Path $PSScriptRoot 'setup-local-database.ps1') -EnvironmentFile $testEnv
$prior = @{}
foreach ($key in $settings.Keys) { $prior[$key] = [Environment]::GetEnvironmentVariable($key); [Environment]::SetEnvironmentVariable($key,$settings[$key],'Process') }
$names = @('HTQLHDK_APP_ROOT','ASPNETCORE_ENVIRONMENT','HTQLHDK_TEST_URL','HTQLHDK_TEST_ROOT','HTQLHDK_TEST_DATABASE_URL')
foreach ($key in $names) { $prior[$key] = [Environment]::GetEnvironmentVariable($key) }
$process = $null
try {
    $env:HTQLHDK_APP_ROOT = $testRoot
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:HTQLHDK_TEST_URL = "http://127.0.0.1:$Port"
    $env:HTQLHDK_TEST_ROOT = $testRoot
    $api = Join-Path $root 'src/backend/Htqlhdk.Api/bin/Release/net10.0/Htqlhdk.Api.dll'
    & dotnet $api --seed-local
    if ($LASTEXITCODE -ne 0) { throw 'Test database seed failed.' }
    if (Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue) { throw 'Test API port is already in use.' }
    $process = Start-Process -FilePath (Get-Command dotnet).Source -ArgumentList ('"' + $api + '" --urls ' + $env:HTQLHDK_TEST_URL) -WindowStyle Hidden -PassThru -WorkingDirectory (Join-Path $root 'src/backend/Htqlhdk.Api') -RedirectStandardOutput (Join-Path $testRoot 'api.stdout.log') -RedirectStandardError (Join-Path $testRoot 'api.stderr.log')
    $ready = $false
    for ($attempt=0;$attempt -lt 30;$attempt++) {
        try { $ready = (Invoke-WebRequest -Uri ($env:HTQLHDK_TEST_URL + '/health/ready') -UseBasicParsing -NoProxy -TimeoutSec 2).StatusCode -eq 200 }
        catch { if ($attempt -eq 0) { Write-Output "Waiting for test API: $($_.Exception.GetType().Name)" } }
        if ($ready) { break }; Start-Sleep -Milliseconds 500
    }
    if (-not $ready) { throw 'Isolated test API did not become ready.' }
    & node (Join-Path $root 'tests/core13/http-flow.mjs')
    if ($LASTEXITCODE -ne 0) { throw 'Core13 HTTP integration checks failed.' }
    $env:HTQLHDK_TEST_DATABASE_URL = "Host=$($settings['HTQLHDK_DB_HOST']);Port=$($settings['HTQLHDK_DB_PORT']);Database=$($settings['HTQLHDK_DB_NAME']);Username=$($settings['HTQLHDK_DB_APP_USER']);Password=$($settings['HTQLHDK_DB_APP_PASSWORD']);Timezone=UTC"
    & dotnet test (Join-Path $root 'tests/Htqlhdk.Worker.Tests/Htqlhdk.Worker.Tests.csproj') --configuration Release --no-build --filter 'TestCategory=DatabaseIntegration'
    if ($LASTEXITCODE -ne 0) { throw 'Core13 worker integration checks failed.' }
}
finally {
    if ($process -and -not $process.HasExited) { Stop-Process -Id $process.Id }
    foreach ($key in $prior.Keys) { [Environment]::SetEnvironmentVariable($key,$prior[$key],'Process') }
}
