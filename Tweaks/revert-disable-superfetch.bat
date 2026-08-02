@echo off
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters" /v "EnableSuperfetch" /t REG_DWORD /d 3 /f
if errorlevel 1 goto :fail
sc config SysMain start= auto
sc start SysMain >nul 2>&1
echo Done.
exit /b 0
:fail
echo Failed to revert Superfetch.
exit /b 1
