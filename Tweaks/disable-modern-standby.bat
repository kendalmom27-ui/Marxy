@echo off
REM Disable Modern Standby (PlatformAoAcOverride = 0)
REM
REM Modern Standby (S0 Low Power Idle / "AoAc" - Always on, Always Connected)
REM keeps the system in a light, network-connected sleep instead of the older
REM deep S3 sleep. It can cause higher battery drain in bag/standby and, for
REM some users, wake/latency quirks. Setting PlatformAoAcOverride = 0 forces
REM Windows to fall back to traditional S3 sleep where the hardware supports it.
REM
REM TRADEOFF: on machines whose firmware only implements Modern Standby (many
REM modern thin laptops dropped S3 support entirely), disabling it can leave
REM sleep behaving oddly - it may not enter deep sleep at all. Best on desktops
REM or laptops that still expose S3. Restart required.
REM
REM TO REVERT: delete the value ->
REM   reg delete "HKLM\SYSTEM\CurrentControlSet\Control\Power" /v PlatformAoAcOverride /f

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power" /v "PlatformAoAcOverride" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set PlatformAoAcOverride.
exit /b 1
