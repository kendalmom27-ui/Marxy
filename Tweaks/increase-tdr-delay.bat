@echo off
REM Increase GPU Timeout Delay (TdrDelay)
REM
REM Windows normally assumes a GPU has crashed if it doesn't respond within
REM 2 seconds (default) and force-resets the driver (a "TDR" / driver
REM crash-recovery event). This raises that threshold to 8 seconds, which
REM prevents false-positive resets during heavy overclocking, GPU compute
REM workloads, or benchmarking where the GPU is legitimately busy rather
REM than actually hung.
REM
REM HONEST NOTE: Microsoft's own documentation states this registry key is
REM intended for driver developers/debugging, not end-user tuning. It's
REM commonly used by enthusiasts for the reason above, but it's not an
REM officially endorsed "gaming tweak".
REM
REM TRADEOFF: if your GPU driver genuinely does hang, your screen will
REM stay frozen longer before Windows recovers it, since the timeout is
REM longer. A restart is required for full effect.

reg add "HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers" /v "TdrDelay" /t REG_DWORD /d 8 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set TdrDelay.
exit /b 1
