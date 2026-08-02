@echo off
REM Hidden NIC Advanced Settings
REM Runs hidden-nic-settings.ps1 in this same folder - see that file for
REM full details on what each property does.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0hidden-nic-settings.ps1"
if errorlevel 1 goto :fail

echo Done.
exit /b 0

:fail
echo Failed - see output above.
exit /b 1
