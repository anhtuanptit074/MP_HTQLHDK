param(
    [switch]$LoadTestFixture
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
$envFile = Join-Path $root '.env.local'

if (-not (Test-Path -LiteralPath $envFile)) {
    throw 'Missing .env.local. Copy .env.local.example and replace every REPLACE_* value first.'
}

$settings = @{}
Get-Content -LiteralPath $envFile -Encoding UTF8 | ForEach-Object {
    $line = $_.Trim()
    if ($line -and -not $line.StartsWith('#')) {
        $parts = $line.Split('=', 2)
        if ($parts.Count -eq 2 -and $parts[0] -match '^HTQLHDK_[A-Z0-9_]+$') {
            $settings[$parts[0]] = $parts[1]
        }
    }
}

foreach ($required in @('HTQLHDK_DB_NAME', 'HTQLHDK_DB_ADMIN_USER', 'HTQLHDK_DB_ADMIN_PASSWORD')) {
    if (-not $settings.ContainsKey($required) -or [string]::IsNullOrWhiteSpace($settings[$required])) {
        throw "Missing $required in .env.local."
    }
}

if ($settings['HTQLHDK_DB_ADMIN_PASSWORD'] -like 'REPLACE_*') {
    throw 'Replace the local database password placeholder before starting PostgreSQL.'
}

Push-Location $root
try {
    & docker compose --env-file $envFile up -d --wait postgres
    if ($LASTEXITCODE -ne 0) { throw 'PostgreSQL did not become healthy.' }

    $baseArgs = @(
        'compose', '--env-file', $envFile, 'exec', '-T', 'postgres',
        'psql', '-v', 'ON_ERROR_STOP=1',
        '-U', $settings['HTQLHDK_DB_ADMIN_USER'],
        '-d', $settings['HTQLHDK_DB_NAME']
    )

    & docker @baseArgs -f '/workspace/ops/migrations/V001__baseline.psql'
    if ($LASTEXITCODE -ne 0) { throw 'Migration V001 failed.' }

    & docker @baseArgs -f '/workspace/ops/migrations/verify_baseline.psql'
    if ($LASTEXITCODE -ne 0) { throw 'Baseline verification failed.' }

    if ($LoadTestFixture) {
        if ($settings['HTQLHDK_DB_NAME'] -match '(?i)prod|production') {
            throw 'Refusing to load a fixture into a production-like database name.'
        }
        & docker @baseArgs -f '/workspace/tests/fixtures/sql/load_test_fixture.psql'
        if ($LASTEXITCODE -ne 0) { throw 'Test fixture failed.' }
    }
}
finally {
    Pop-Location
}

