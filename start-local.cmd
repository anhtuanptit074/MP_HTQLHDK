@echo off
setlocal
set "HTQLHDK_PWSH="
for /f "delims=" %%P in ('where pwsh.exe 2^>nul') do if not defined HTQLHDK_PWSH set "HTQLHDK_PWSH=%%P"
if not defined HTQLHDK_PWSH set "HTQLHDK_PWSH=%USERPROFILE%\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\powershell\pwsh.exe"
if not exist "%HTQLHDK_PWSH%" (
    echo PowerShell 7 is required. Install PowerShell 7 and try again.
    exit /b 1
)
"%HTQLHDK_PWSH%" -NoProfile -File "%~dp0scripts\start-local.ps1" %*
exit /b %errorlevel%
