@echo off
REM Optimize Memory for Programs (LargeSystemCache)
REM
REM Controls how Windows splits physical RAM between the file system cache
REM and application working sets. This is the exact registry value behind
REM the old "Programs" vs "System cache" radio buttons under System
REM Properties > Advanced > Performance Settings > Advanced (removed from
REM the GUI on modern Windows, but the key still has effect):
REM   0 = favor application/program memory (client/gaming-oriented default)
REM   1 = favor file system cache (file-server/LAN throughput oriented)
REM
REM 0 is already the out-of-box default on Windows client installs, so this
REM is mainly useful if something else (a "LAN performance" guide, a prior
REM tool, Server-oriented tuning advice) set it to 1 on your system - this
REM makes sure it's explicitly back on the gaming-appropriate setting.

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "LargeSystemCache" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set LargeSystemCache.
exit /b 1
