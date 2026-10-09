$ErrorActionPreference = 'Stop'

$localToolsRoot = Split-Path -Parent $PSScriptRoot
$localSdk = Join-Path $localToolsRoot '.codex_tmp/dotnet'
if (Test-Path -LiteralPath (Join-Path $localSdk 'dotnet.exe')) {
    $env:DOTNET_ROOT = $localSdk
    $env:PATH = $localSdk + [IO.Path]::PathSeparator + $env:PATH
    if (-not $env:DOTNET_CLI_HOME) {
        $env:DOTNET_CLI_HOME = Join-Path $localToolsRoot '.codex_tmp/dotnet-home'
    }
    if (-not $env:NUGET_PACKAGES) {
        $env:NUGET_PACKAGES = Join-Path $localToolsRoot '.codex_tmp/nuget-packages'
    }
}

if ($PSVersionTable.PSVersion.Major -ge 7) {
    $env:PATH = $PSHOME + [IO.Path]::PathSeparator + $env:PATH
}
elseif (-not (Get-Command pwsh -ErrorAction SilentlyContinue)) {
    $bundledPowerShell = Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell'
    if (Test-Path -LiteralPath (Join-Path $bundledPowerShell 'pwsh.exe')) {
        $env:PATH = $bundledPowerShell + [IO.Path]::PathSeparator + $env:PATH
    }
}
