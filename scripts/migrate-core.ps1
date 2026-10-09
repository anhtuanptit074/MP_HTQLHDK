param([string]$EnvironmentFile)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path -Parent $PSScriptRoot
$envFile = if ($EnvironmentFile) { $EnvironmentFile } else { Join-Path $root '.env.local' }
$settings = @{}
Get-Content -LiteralPath $envFile -Encoding UTF8 | ForEach-Object { if ($_ -match '^([A-Z0-9_]+)=(.*)$') { $settings[$matches[1]] = $matches[2] } }
foreach ($key in @('HTQLHDK_DB_NAME','HTQLHDK_DB_ADMIN_USER')) {
    if ($settings[$key] -notmatch '^[a-zA-Z_][a-zA-Z0-9_]*$') { throw "Invalid local identifier: $key" }
}
if ($settings['HTQLHDK_ENVIRONMENT'] -ne 'Local' -or $settings['HTQLHDK_DB_NAME'] -notmatch '_local$') { throw 'This migration helper is restricted to Local databases ending in _local.' }
$manifest = Get-Content -LiteralPath (Join-Path $root 'ops/core-migrations/V001__manifest.json') -Raw | ConvertFrom-Json
$source = [IO.File]::ReadAllText((Join-Path $root $manifest.file)).Replace("`r`n","`n").Replace("`r","`n")
$hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($source))).ToLowerInvariant()
if ($hash -ne $manifest.sha256) { throw 'Core13 migration checksum mismatch.' }
Push-Location $root
try {
    $containerEnv = if ($EnvironmentFile) { Join-Path $root '.env.local' } else { $envFile }
    & docker compose --env-file $containerEnv up -d --wait postgres
    if ($LASTEXITCODE -ne 0) { throw 'PostgreSQL did not become healthy.' }
    $admin = $settings['HTQLHDK_DB_ADMIN_USER']; $database = $settings['HTQLHDK_DB_NAME']
    $create = "SELECT format('CREATE DATABASE %I OWNER %I', '$database', '$admin') WHERE NOT EXISTS(SELECT 1 FROM pg_database WHERE datname='$database') \gexec"
    $create | & docker compose --env-file $envFile exec -T postgres psql -v ON_ERROR_STOP=1 -U $admin -d postgres
    if ($LASTEXITCODE -ne 0) { throw 'Core13 database creation failed.' }
    $baseArgs = @('compose','--env-file',$envFile,'exec','-T','postgres','psql','-v','ON_ERROR_STOP=1','-U',$admin,'-d',$database)
    $comment = ((& docker @baseArgs -At -c "SELECT coalesce(obj_description(oid,'pg_namespace'),'unversioned') FROM pg_namespace WHERE nspname='htqlhdk'") -join "`n").Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Cannot read schema version.' }
    $expected = "HTQLHDK core13 V001 sha256=$hash"
    if ($comment) {
        if ($comment -ne $expected -and -not $comment.StartsWith("$expected V002 sha256=",[StringComparison]::Ordinal)) { throw 'Existing schema is incompatible; no data was changed.' }
        Write-Output 'Core13 V001 already applied; no changes made.'
    }
    else {
        ($source -replace 'COMMIT;\s*$', "COMMENT ON SCHEMA htqlhdk IS '$expected';`nCOMMIT;") | & docker @baseArgs
        if ($LASTEXITCODE -ne 0) { throw 'Core13 V001 failed.' }
    }
    $second = Get-Content -LiteralPath (Join-Path $root 'ops/core-migrations/V002__manifest.json') -Raw | ConvertFrom-Json
    $secondSource = [IO.File]::ReadAllText((Join-Path $root $second.file)).Replace("`r`n","`n").Replace("`r","`n")
    $secondHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($secondSource))).ToLowerInvariant()
    if ($secondHash -ne $second.sha256) { throw 'Core13 V002 checksum mismatch.' }
    $current = ((& docker @baseArgs -At -c "SELECT obj_description(oid,'pg_namespace') FROM pg_namespace WHERE nspname='htqlhdk'") -join "`n").Trim()
    $secondExpected = "$expected V002 sha256=$secondHash"
    if ($current -eq $expected) {
        ($secondSource -replace 'COMMIT;\s*$', "COMMENT ON SCHEMA htqlhdk IS '$secondExpected';`nCOMMIT;") | & docker @baseArgs
        if ($LASTEXITCODE -ne 0) { throw 'Core13 V002 failed.' }
    }
    elseif ($current -ne $secondExpected) { throw 'Core13 V002 incompatible version.' }
    $verify = 'DO $$ BEGIN IF (SELECT count(*) FROM information_schema.tables WHERE table_schema=''htqlhdk'' AND table_type=''BASE TABLE'') <> 13 THEN RAISE EXCEPTION ''Expected exactly 13 business tables''; END IF; END $$;'
    $verify | & docker @baseArgs
    if ($LASTEXITCODE -ne 0) { throw 'Core13 verification failed.' }
    Write-Output 'PASS: core13 database; exactly 13 business tables.'
}
finally { Pop-Location }
