param([switch]$LoadTestFixture)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path -Parent $PSScriptRoot
$isCore = (Get-Content -LiteralPath (Join-Path $root '.env.local')) -match '^HTQLHDK_SCHEMA_MODEL=core13$'
if ($isCore) {
    & (Join-Path $PSScriptRoot 'migrate-core.ps1')
    if ($LoadTestFixture) { Write-Output 'Core13 sample data is seeded safely by start-local.cmd; no data reset performed.' }
}
else { & (Join-Path $PSScriptRoot 'migrate-legacy.ps1') -LoadTestFixture:$LoadTestFixture }
