@echo off
REM Enable Hardware-Accelerated GPU Scheduling (HAGS)
REM
REM Gives the GPU its own hardware scheduler instead of routing every
REM command through the CPU-managed software scheduler. Can reduce CPU
REM overhead and input latency on supported hardware (requires a
REM WDDM 2.7+ driver, Windows 10 2004+).
REM
REM TRADEOFF: results genuinely vary by GPU/driver combination - some
REM systems see improvement, others see stutter or no change. If you
REM notice regressions after enabling this, it's safe to turn back off
REM (HwSchMode=1) with no lasting effect. A restart is required either way.

reg add "HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers" /v "HwSchMode" /t REG_DWORD /d 2 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set HwSchMode.
exit /b 1
