@echo off
REM Enable MSI Mode for GPU
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0enable-msi-mode.ps1"
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed - see output above.
exit /b 1
