$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot 'use-local-tools.ps1')

$requirements = @(
    @{ Name = 'Git'; Command = 'git' },
    @{ Name = 'Node.js'; Command = 'node' },
    @{ Name = 'npm'; Command = 'npm.cmd' },
    @{ Name = '.NET SDK'; Command = 'dotnet' },
    @{ Name = 'Docker'; Command = 'docker' }
)

$missing = @()
foreach ($requirement in $requirements) {
    if (-not (Get-Command $requirement.Command -ErrorAction SilentlyContinue)) {
        $missing += $requirement.Name
    }
}

if ($missing.Count -gt 0) {
    throw "Missing required tools: $($missing -join ', '). See docs/development/01_local_setup.md."
}

$root = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path -LiteralPath (Join-Path $root '.env.local'))) {
    Write-Warning 'Create .env.local from .env.local.example and replace both local password placeholders.'
}

Write-Output 'PASS: required developer tools are available.'
