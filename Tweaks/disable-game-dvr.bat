@echo off
REM Disable Game DVR / Xbox Game Bar
REM
REM Game DVR hooks into the DirectX presentation pipeline to enable
REM background recording, Win+G overlay, and clip capture. This overhead
REM runs even if you never actually use it. Confirmed against Microsoft's
REM own Q&A documentation and consistent across many independent sources.
REM
REM TRADEOFF: you lose Xbox Game Bar's built-in recording/screenshot
REM features (Win+G overlay, background clip capture). If you use those,
REM don't run this. If you use a separate tool (OBS, ShadowPlay, etc.)
REM for recording, you don't need Windows' built-in version running too.

reg add "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR" /v "AppCaptureEnabled" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

reg add "HKCU\System\GameConfigStore" /v "GameDVR_Enabled" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR" /v "AllowGameDVR" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

echo Done. Restart recommended for full effect.
exit /b 0

:fail
echo Failed to disable Game DVR.
exit /b 1
