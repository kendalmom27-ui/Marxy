@echo off
REM Distribute Timer Interrupts (DistributeTimers = 1)
REM
REM By default Windows can concentrate clock-tick / timer interrupt handling
REM on CPU 0. Setting DistributeTimers = 1 lets the kernel spread those timer
REM interrupts across all cores, reducing contention on CPU 0 and the small
REM synchronization delays that come from piling timer work on one core.
REM Real, settable kernel value; generally low-risk.
REM
REM Restart required for full effect.
REM
REM TO REVERT: set it back to 0 ->
REM   reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel" /v DistributeTimers /t REG_DWORD /d 0 /f

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel" /v "DistributeTimers" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set DistributeTimers.
exit /b 1
