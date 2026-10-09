param([string]$EnvironmentFile)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path -Parent $PSScriptRoot
$envFile = if ($EnvironmentFile) { $EnvironmentFile } else { Join-Path $root '.env.local' }
$settings = @{}
Get-Content -LiteralPath $envFile -Encoding UTF8 | ForEach-Object {
    if ($_ -match '^([A-Z0-9_]+)=(.*)$') { $settings[$matches[1]] = $matches[2] }
}

if ($settings['HTQLHDK_ENVIRONMENT'] -ne 'Local' -or $settings['HTQLHDK_DB_NAME'] -notmatch '_local$') {
    throw 'Runtime account setup is restricted to Local databases ending in _local.'
}
foreach ($key in @('HTQLHDK_DB_APP_USER', 'HTQLHDK_DB_ADMIN_USER', 'HTQLHDK_DB_NAME')) {
    if ($settings[$key] -notmatch '^[a-zA-Z_][a-zA-Z0-9_]*$') { throw "Invalid local identifier: $key" }
}
if ([string]::IsNullOrWhiteSpace($settings['HTQLHDK_DB_APP_PASSWORD']) -or $settings['HTQLHDK_DB_APP_PASSWORD'] -like 'REPLACE_*') {
    throw 'Set the application password in .env.local first.'
}
if ($settings['HTQLHDK_DB_APP_USER'] -eq $settings['HTQLHDK_DB_ADMIN_USER']) {
    throw 'Application and migration accounts must be different.'
}

Push-Location $root
try {
    if ($settings['HTQLHDK_SCHEMA_MODEL'] -eq 'core13') { & (Join-Path $PSScriptRoot 'migrate-core.ps1') -EnvironmentFile $envFile }
    else { & (Join-Path $PSScriptRoot 'migrate.ps1') }
    $role = $settings['HTQLHDK_DB_APP_USER']
    $database = $settings['HTQLHDK_DB_NAME']
    $escapedPassword = $settings['HTQLHDK_DB_APP_PASSWORD'].Replace("'", "''")
    # Send the password via stdin; do not put it in a file or process arguments.
    $sql = @"
\set ON_ERROR_STOP on
BEGIN;
SELECT format('CREATE ROLE %I LOGIN', '$role')
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '$role') \gexec
ALTER ROLE "$role" WITH LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS PASSWORD '$escapedPassword';
GRANT CONNECT ON DATABASE "$database" TO "$role";
GRANT USAGE ON SCHEMA htqlhdk TO "$role";
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA htqlhdk TO "$role";
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA htqlhdk TO "$role";
ALTER ROLE "$role" IN DATABASE "$database" SET search_path = htqlhdk, public;
ALTER ROLE "$role" IN DATABASE "$database" SET timezone = 'UTC';
COMMIT;
"@
    $sql | & docker compose --env-file $envFile exec -T postgres psql -v ON_ERROR_STOP=1 `
        -U $settings['HTQLHDK_DB_ADMIN_USER'] -d $database
    if ($LASTEXITCODE -ne 0) { throw 'Local runtime account setup failed.' }

    $previousPassword = $env:PGPASSWORD
    try {
        $env:PGPASSWORD = $settings['HTQLHDK_DB_APP_PASSWORD']
        & docker compose --env-file $envFile exec -T -e PGPASSWORD postgres psql -v ON_ERROR_STOP=1 `
            -h 127.0.0.1 -U $role -d $database `
            -c 'SELECT current_user, count(*) AS test_assets FROM htqlhdk.asset;'
        if ($LASTEXITCODE -ne 0) { throw 'Application database login failed.' }
    }
    finally { $env:PGPASSWORD = $previousPassword }
    Write-Output 'PASS: local runtime database account can log in and read application data.'
}
finally { Pop-Location }
