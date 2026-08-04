@echo off
REM Disable AMD ULPS (Ultra Low Power State)
REM Runs disable-amd-ulps.ps1 in this same folder - see it for details. Only
REM affects AMD/Radeon GPUs; safely does nothing on NVIDIA/Intel systems.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0disable-amd-ulps.ps1"
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed - see output above.
exit /b 1
