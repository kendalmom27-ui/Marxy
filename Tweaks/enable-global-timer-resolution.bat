@echo off
REM Global Timer Resolution Requests (GlobalTimerResolutionRequests = 1)
REM
REM Starting with Windows 11 (and late Win10 builds), Microsoft changed how
REM timer resolution works: an app requesting a higher timer resolution (e.g.
REM a game asking for 1ms via timeBeginPeriod) now only affects that process,
REM not the whole system. This value restores the OLD global behavior, so a
REM high timer resolution requested by any app applies system-wide - which is
REM what many latency guides assume and what tools measuring system timer
REM resolution expect. Real, known key.
REM
REM TRADEOFF: a global high timer resolution slightly increases power draw /
REM reduces the CPU's ability to enter deep idle, since the system tick runs
REM more often. Restart required.
REM
REM TO REVERT: delete the value ->
REM   reg delete "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel" /v GlobalTimerResolutionRequests /f

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel" /v "GlobalTimerResolutionRequests" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set GlobalTimerResolutionRequests.
exit /b 1
