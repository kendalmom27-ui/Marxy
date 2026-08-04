@echo off
REM Increase GPU DDI Timeout (TdrDdiDelay)
REM
REM Companion to TdrDelay. Where TdrDelay controls how long Windows waits for
REM the GPU overall before declaring a TDR (driver hang) and resetting it,
REM TdrDdiDelay controls how long it waits specifically for the display
REM driver's individual DDI calls to return. Raising it to 8 seconds matches
REM the TdrDelay tweak so both thresholds move together, preventing false-
REM positive driver resets during heavy/legitimate GPU load. Same documented
REM GraphicsDrivers key family as TdrDelay.
REM
REM TRADEOFF: like TdrDelay, if the GPU genuinely hangs your screen stays
REM frozen a bit longer before Windows recovers it, since the timeout is
REM higher. A restart is required.
REM
REM TO REVERT: delete the value ->
REM   reg delete "HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers" /v TdrDdiDelay /f

reg add "HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers" /v "TdrDdiDelay" /t REG_DWORD /d 8 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set TdrDdiDelay.
exit /b 1
