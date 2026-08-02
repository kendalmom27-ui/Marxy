@echo off
REM Enable Long Paths (removes 260-character path limit)
REM Officially documented Microsoft feature (Windows 10 1607+). Lets apps
REM that support it work with file paths longer than 260 characters -
REM useful for deep folder structures, some game mod managers, and dev
REM tools like git. No real downside; a restart is required.

reg add "HKLM\SYSTEM\CurrentControlSet\Control\FileSystem" /v "LongPathsEnabled" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set LongPathsEnabled.
exit /b 1
