@echo off
REM Keep Kernel & Drivers in RAM (DisablePagingExecutive)
REM
REM Prevents Windows from paging kernel-mode drivers and system code to
REM disk when idle, keeping them resident in physical RAM instead.
REM Documented Microsoft setting (used historically for 64-bit stack
REM walking on Windows 7/Vista per Microsoft's own Kernel Trace Control
REM docs).
REM
REM HONEST NOTE: this mattered most on older Windows versions and
REM RAM-constrained systems. On modern Windows 10/11 with 16GB+ RAM, the
REM real-world performance benefit is disputed - several sources report
REM it's closer to placebo today. Included because it's harmless on
REM systems with adequate RAM, not because it's guaranteed to help.
REM
REM TRADEOFF: uses more RAM at all times; not recommended on low-RAM
REM systems (under ~8GB). A restart is required to take effect.

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "DisablePagingExecutive" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set DisablePagingExecutive.
exit /b 1
