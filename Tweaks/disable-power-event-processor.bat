@echo off
REM Disable Power Event Processor (EventProcessorEnabled)
REM
REM The power event processor is the kernel component that reacts to
REM "power efficiency" events at runtime - dynamically adjusting processor
REM performance state and core parking behavior in response to workload and
REM utilization changes. Setting this to 0 stops that dynamic adjustment, so
REM the CPU holds a steadier performance state instead of being continually
REM re-evaluated and stepped up/down.
REM
REM Default is 1 (enabled) on a stock Windows install.
REM
REM TRADEOFF: real increase in idle power draw and heat, since the kernel
REM stops stepping the CPU down opportunistically. Same category of tradeoff
REM as "Disable CPU Power Saving" - not recommended on laptops running on
REM battery.
REM
REM HONEST NOTE: this value is undocumented by Microsoft (it's sourced from
REM community tweak packs, not official docs). It's real and settable, but
REM its exact individual effect isn't independently verified the way the
REM documented powercfg-based tweaks in this app are.

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power" /v "EventProcessorEnabled" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set EventProcessorEnabled.
exit /b 1
