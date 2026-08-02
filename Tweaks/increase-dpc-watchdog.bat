@echo off
REM Increase DPC Watchdog Timeout
REM
REM The Windows kernel has a built-in watchdog that monitors how long any
REM single Deferred Procedure Call (DPC) runs at DISPATCH_LEVEL or above.
REM If a driver's DPC runs too long, the kernel force-crashes the system
REM with DPC_WATCHDOG_VIOLATION (bugcheck 0x133) - this value is read
REM directly from the kernel's KPRCB structure, confirmed via Microsoft's
REM own driver debugging documentation.
REM
REM Raising these values gives legitimately busy drivers (heavy network
REM load, certain audio interfaces, GPU drivers under load) more time
REM before the kernel assumes something is hung and crashes the system.
REM
REM DpcWatchdogPeriod = 0x0001D4C0 (120,000 ticks, ~2 minutes)
REM DpcTimeout        = 0x00004E20 (20,000 ticks, ~20 seconds per single DPC)
REM
REM TRADEOFF: if a driver genuinely does hang, your system will stay
REM frozen longer before the kernel catches it and recovers/crashes,
REM since the safety threshold is now higher. A restart is required.

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Kernel" /v "DpcWatchdogPeriod" /t REG_DWORD /d 0x0001D4C0 /f
if errorlevel 1 goto :fail

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Kernel" /v "DpcTimeout" /t REG_DWORD /d 0x00004E20 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set DPC watchdog registry values.
exit /b 1
