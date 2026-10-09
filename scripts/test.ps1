param(
    [switch]$IncludeDatabase
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot 'use-local-tools.ps1')

$root = Split-Path -Parent $PSScriptRoot

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw '.NET SDK 10.0.400 is required to build the backend.'
}

Push-Location $root
try {
    Push-Location 'src/frontend'
    try {
        & npm.cmd ci
        if ($LASTEXITCODE -ne 0) { throw 'Frontend dependency restore failed.' }
    }
    finally {
        Pop-Location
    }

    & (Join-Path $PSScriptRoot 'verify-stage10.ps1')
    if ($LASTEXITCODE -ne 0) { throw 'Stage-10 structural verification failed.' }

    & dotnet restore HTQLHDK.slnx --configfile (Join-Path $root 'NuGet.Config')
    if ($LASTEXITCODE -ne 0) { throw 'Backend dependency restore failed.' }
    & dotnet build HTQLHDK.slnx --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Backend build failed.' }
    & dotnet test HTQLHDK.slnx --configuration Release --no-build --filter 'TestCategory!=DatabaseIntegration'
    if ($LASTEXITCODE -ne 0) { throw 'Backend tests failed.' }

    Push-Location 'src/frontend'
    try {
        & npm.cmd run lint
        if ($LASTEXITCODE -ne 0) { throw 'Frontend lint failed.' }
        & npm.cmd run format:check
        if ($LASTEXITCODE -ne 0) { throw 'Frontend format check failed.' }
        & npm.cmd test
        if ($LASTEXITCODE -ne 0) { throw 'Frontend tests failed.' }
        & npm.cmd run build
        if ($LASTEXITCODE -ne 0) { throw 'Frontend build failed.' }
    }
    finally {
        Pop-Location
    }

    if ($IncludeDatabase) {
        & (Join-Path $PSScriptRoot 'migrate.ps1') -LoadTestFixture
        if ($LASTEXITCODE -ne 0) { throw 'Database migration/fixture test failed.' }
        & (Join-Path $PSScriptRoot 'test-core13.ps1')
    }
}
finally {
    Pop-Location
}

Write-Output 'PASS: stage 10 build and test pipeline completed.'
