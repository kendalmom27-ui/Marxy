@echo off
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "DisablePagingExecutive" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail
echo Done. Restart required.
exit /b 0
:fail
echo Failed to revert DisablePagingExecutive.
exit /b 1
