@echo off
REM Disable Delivery Optimization Internet P2P
REM
REM Windows Update has a peer-to-peer feature on by default that uploads
REM Windows Update/Store data from your PC to strangers on the internet
REM (not just your local network) to help distribute updates faster for
REM everyone. This is buried in Settings > Windows Update > Advanced
REM options > Delivery Optimization - most people never find it.
REM
REM DODownloadMode=0 disables peer-to-peer sharing entirely while still
REM using Delivery Optimization's HTTP-based delivery - you still get
REM updates normally, you just stop uploading to other people's PCs.

reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization" /v "DODownloadMode" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

echo Done. Restart recommended for full effect.
exit /b 0

:fail
echo Failed to set DODownloadMode.
exit /b 1
