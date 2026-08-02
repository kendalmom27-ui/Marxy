@echo off
reg add "HKCU\Control Panel\Desktop" /v "MenuShowDelay" /t REG_SZ /d "400" /f
if errorlevel 1 goto :fail
echo Done. Sign out and back in.
exit /b 0
:fail
echo Failed to revert MenuShowDelay.
exit /b 1
