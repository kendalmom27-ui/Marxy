@echo off
REM Force Game Mode Always On
REM
REM Game Mode already has a toggle in Settings > Gaming, but there's a
REM well-documented, recurring issue (multiple current Microsoft Q&A
REM threads) where it silently turns itself back off mid-session, causing
REM stutter. This sets it at both the user registry level and the policy
REM level, which is more reliable against getting flipped off unexpectedly
REM than just clicking the Settings toggle once.

reg add "HKCU\Software\Microsoft\GameBar" /v "AutoGameModeEnabled" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR" /v "AllowAutoGameMode" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

echo Done. Restart recommended for full effect.
exit /b 0

:fail
echo Failed to set Game Mode registry values.
exit /b 1
