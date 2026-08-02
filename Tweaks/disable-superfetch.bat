@echo off
REM Disable Superfetch (SysMain)
REM
REM Superfetch proactively preloads frequently-used apps into RAM to
REM speed up load times - genuinely useful on slow HDDs, much less useful
REM on fast SSDs/NVMe where load times are already low, while it still
REM costs background disk I/O and CPU. A well-known, real tweak with zero
REM GUI exposure anywhere in Windows - purely registry and service
REM controlled.
REM
REM Sets the registry value AND stops/disables the SysMain service
REM directly, since the registry value alone doesn't always reliably
REM stop the service on its own.

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters" /v "EnableSuperfetch" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

sc stop SysMain >nul 2>&1
sc config SysMain start= disabled
if errorlevel 1 goto :fail

echo Done.
exit /b 0

:fail
echo Failed to disable Superfetch/SysMain.
exit /b 1
