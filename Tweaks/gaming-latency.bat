@echo off
REM Gaming Latency Tweaks (boot + scheduler + display)
REM
REM - disabledynamictick / useplatformtick: keeps the system timer from
REM   dynamically adjusting its tick rate to save power - trades a small
REM   amount of power efficiency for more consistent timing.
REM - SystemResponsiveness=0: tells Windows' multimedia scheduler to give
REM   foreground apps (like games) full priority instead of reserving a
REM   percentage of CPU for background multimedia tasks. Documented
REM   Microsoft setting; the main real-world tradeoff is that background
REM   audio/video can stutter more under heavy load.
REM - DXGKrnl MonitorLatencyTolerance / MonitorRefreshLatencyTolerance = 0:
REM   tells the display driver not to add extra buffering delay - used in
REM   several official GPU vendor low-latency guides.
REM
REM Requires a restart to fully take effect (bcdedit changes especially).

bcdedit /set disabledynamictick yes
if errorlevel 1 goto :fail

bcdedit /set useplatformtick yes
if errorlevel 1 goto :fail

reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v "SystemResponsiveness" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

reg add "HKLM\SYSTEM\CurrentControlSet\Services\DXGKrnl" /v "MonitorLatencyTolerance" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

reg add "HKLM\SYSTEM\CurrentControlSet\Services\DXGKrnl" /v "MonitorRefreshLatencyTolerance" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo One or more commands failed.
exit /b 1
