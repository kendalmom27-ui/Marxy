@echo off
REM Enable Cache-Aware Scheduling (CacheAwareScheduling = 1)
REM
REM Intended to make the thread scheduler keep threads within specific CPU
REM cache boundaries (favoring cache locality) instead of moving them freely
REM across cores. Set under Session Manager\kernel.
REM
REM This value doesn't exist by default and whether current Windows honors it
REM is uncertain, so treat any benefit as unconfirmed - it's low risk to set.
REM Restart required.
REM
REM TO REVERT: delete the value ->
REM   reg delete "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel" /v CacheAwareScheduling /f

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel" /v "CacheAwareScheduling" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set CacheAwareScheduling.
exit /b 1
