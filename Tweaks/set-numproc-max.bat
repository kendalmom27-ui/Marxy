@echo off
REM Set Boot Processor Count to Detected Maximum
REM Runs set-numproc-max.ps1 in this same folder - see that file for details
REM on why this detects the count at run time instead of hardcoding one.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0set-numproc-max.ps1"
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed - see output above.
exit /b 1
