@echo off
REM Revert HAGS to Windows default (disabled)
reg add "HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers" /v "HwSchMode" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail
echo Done. Restart required.
exit /b 0
:fail
echo Failed to revert HwSchMode.
exit /b 1
