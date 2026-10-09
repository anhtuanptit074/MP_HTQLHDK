param(
    [switch]$Stop,
    [switch]$EnableZap
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'use-local-tools.ps1')

$root = Split-Path -Parent $PSScriptRoot
$runDirectory = Join-Path $root '.codex_tmp/local-run'
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null

if ($Stop) {
    foreach ($service in @('worker', 'api', 'frontend')) {
        $pidFile = Join-Path $runDirectory "$service.pid"
        if (-not (Test-Path -LiteralPath $pidFile)) { continue }
        $serviceId = [int](Get-Content -LiteralPath $pidFile -Raw)
        $process = Get-CimInstance Win32_Process -Filter "ProcessId = $serviceId"
        if ($process -and $process.CommandLine -and $process.CommandLine.Contains($root)) {
            Stop-Process -Id $serviceId
            Write-Output "Stopped $service."
        }
        Remove-Item -LiteralPath $pidFile
    }
    Write-Output 'API/frontend/worker stopped. PostgreSQL remains available.'
    exit 0
}

function Test-LocalService([string]$Url, [string]$ExpectedText) {
    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -NoProxy -TimeoutSec 2
        return $response.StatusCode -eq 200 -and $response.Content.Contains($ExpectedText)
    }
    catch { return $false }
}
function Test-WorkspaceWorker([string]$PidFile) {
    if (-not (Test-Path -LiteralPath $PidFile)) { return $false }
    $workerProcess = Get-CimInstance Win32_Process -Filter "ProcessId = $([int](Get-Content -LiteralPath $PidFile))"
    return $workerProcess -and $workerProcess.CommandLine -and $workerProcess.CommandLine.Contains($root) -and $workerProcess.CommandLine.Contains('Htqlhdk.Worker.dll')
}

Push-Location $root
try {
    & (Join-Path $PSScriptRoot 'bootstrap-local.ps1')
    & (Join-Path $PSScriptRoot 'setup-local-database.ps1')
    if (-not (Test-LocalService 'http://127.0.0.1:5000/' 'core13-ready')) {
        & dotnet build 'src/backend/Htqlhdk.Api/Htqlhdk.Api.csproj' --configuration Release --no-restore
        if ($LASTEXITCODE -ne 0) { throw 'API build failed; run scripts/test.ps1 first.' }
    }
    $workerPidFile = Join-Path $runDirectory 'worker.pid'
    $workerAlive = $false
    $workerAlive = Test-WorkspaceWorker $workerPidFile
    if (-not $workerAlive) {
        & dotnet build 'src/backend/Htqlhdk.Worker/Htqlhdk.Worker.csproj' --configuration Release --no-restore
        if ($LASTEXITCODE -ne 0) { throw 'Worker build failed.' }
    }

    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    Get-Content -LiteralPath (Join-Path $root '.env.local') -Encoding UTF8 | ForEach-Object {
        if ($_ -match '^(HTQLHDK_[A-Z0-9_]+)=(.*)$') {
            [Environment]::SetEnvironmentVariable($matches[1], $matches[2], 'Process')
        }
    }
    $env:Htqlhdk__Environment = 'Local'
    $env:VITE_HTQLHDK_ENVIRONMENT = 'Local'
    $env:HTQLHDK_ZAP_ENABLED = if ($EnableZap) { 'true' } else { 'false' }
    $env:DOTNET_ENVIRONMENT = 'Development'
    $env:HTQLHDK_ZAP_WORK_ROOT = Join-Path $root 'local-data/core13/zap-work'
    $env:HTQLHDK_APP_ROOT = $root
    & dotnet (Join-Path $root 'src/backend/Htqlhdk.Api/bin/Release/net10.0/Htqlhdk.Api.dll') --seed-local
    if ($LASTEXITCODE -ne 0) { throw 'Core13 local account setup failed.' }

    $services = @(
        @{
            Name = 'api'; Port = 5000; Url = 'http://127.0.0.1:5000/'; Expected = 'core13-ready'
            Executable = (Get-Command dotnet).Source
            Arguments = '"' + (Join-Path $root 'src/backend/Htqlhdk.Api/bin/Release/net10.0/Htqlhdk.Api.dll') + '" --urls http://127.0.0.1:5000'
            Directory = Join-Path $root 'src/backend/Htqlhdk.Api'
        },
        @{
            Name = 'frontend'; Port = 5173; Url = 'http://127.0.0.1:5173/'; Expected = '<title>HTQLHDK</title>'
            Executable = (Get-Command node).Source
            Arguments = '"' + (Join-Path $root 'src/frontend/node_modules/vite/bin/vite.js') + '" --host 127.0.0.1 --port 5173 --strictPort'
            Directory = Join-Path $root 'src/frontend'
        }
    )

    foreach ($service in $services) {
        if (-not (Test-LocalService $service.Url $service.Expected)) {
            $listener = Get-NetTCPConnection -LocalPort $service.Port -State Listen -ErrorAction SilentlyContinue
            if ($listener) { throw "Port $($service.Port) is already used by another application." }
            $process = Start-Process -FilePath $service.Executable -ArgumentList $service.Arguments `
                -WorkingDirectory $service.Directory -WindowStyle Hidden -PassThru `
                -RedirectStandardOutput (Join-Path $runDirectory "$($service.Name).stdout.log") `
                -RedirectStandardError (Join-Path $runDirectory "$($service.Name).stderr.log")
            Set-Content -LiteralPath (Join-Path $runDirectory "$($service.Name).pid") -Value $process.Id
            for ($attempt = 0; $attempt -lt 30; $attempt++) {
                if (Test-LocalService $service.Url $service.Expected) { break }
                if ($process.HasExited) { break }
                Start-Sleep -Milliseconds 500
            }
            if (-not (Test-LocalService $service.Url $service.Expected)) {
                throw "$($service.Name) did not start. See logs in $runDirectory."
            }
        }
        Write-Output "READY: $($service.Name) $($service.Url)"
    }
    $workerPidFile = Join-Path $runDirectory 'worker.pid'
    $workerAlive = $false
    $workerAlive = Test-WorkspaceWorker $workerPidFile
    $workerMode = Join-Path $runDirectory 'worker.enabled'
    if ($workerAlive -and (Test-Path -LiteralPath $workerMode) -and ((Get-Content -LiteralPath $workerMode -Raw).Trim() -ne [string][bool]$EnableZap)) {
        $currentWorker = Get-CimInstance Win32_Process -Filter "ProcessId = $([int](Get-Content $workerPidFile))"
        if (-not $currentWorker.CommandLine.Contains($root)) { throw 'Worker PID is not owned by this workspace.' }
        Stop-Process -Id $currentWorker.ProcessId
        $workerAlive = $false
    }
    if (-not $workerAlive) {
        $worker = Start-Process -FilePath (Get-Command dotnet).Source -ArgumentList ('"' + (Join-Path $root 'src/backend/Htqlhdk.Worker/bin/Release/net10.0/Htqlhdk.Worker.dll') + '"') -WorkingDirectory (Join-Path $root 'src/backend/Htqlhdk.Worker') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runDirectory 'worker.stdout.log') -RedirectStandardError (Join-Path $runDirectory 'worker.stderr.log')
        Set-Content -LiteralPath $workerPidFile -Value $worker.Id
        Set-Content -LiteralPath $workerMode -Value ([string][bool]$EnableZap)
        Start-Sleep -Milliseconds 700
        if ($worker.HasExited) { throw 'Worker failed to start. See local-run logs.' }
    }
    Write-Output "READY: core13 worker; ZAP dispatch enabled = $EnableZap."
}
finally { Pop-Location }
