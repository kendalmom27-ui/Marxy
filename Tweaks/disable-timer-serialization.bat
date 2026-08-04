@echo off
REM Disable Timer Serialization (SerializeTimerExpiration = 0)
REM
REM Controls whether the kernel serializes (orders) timer expirations through
REM a synchronized path. Setting SerializeTimerExpiration = 0 lets timers fire
REM without waiting on that ordering, which some latency-focused setups prefer.
REM Real, settable kernel value.
REM
REM Real-world benefit is debated / not independently confirmed - low risk, but
REM don't expect a guaranteed improvement. Restart required.
REM
REM TO REVERT: set it back to 1 ->
REM   reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel" /v SerializeTimerExpiration /t REG_DWORD /d 1 /f

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel" /v "SerializeTimerExpiration" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set SerializeTimerExpiration.
exit /b 1
