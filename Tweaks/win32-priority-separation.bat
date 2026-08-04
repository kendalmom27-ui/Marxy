@echo off
REM Foreground Process Priority Boost (Win32PrioritySeparation)
REM
REM Controls how Windows splits CPU time between the active foreground
REM app and background processes. The app passes the chosen preset as the
REM first argument (decimal): 38=0x26, 24=0x18, 22=0x16, 42=0x2A. 0x26 (38)
REM is the most commonly cited gaming value; the others trade quantum length
REM and boost differently. Defaults to 38 if no argument is given.
REM
REM TRADEOFF: multitasking while gaming (streaming, background apps) can
REM feel less smooth since background processes get less CPU time.
REM A restart is required for this to take effect.

set "VAL=%~1"
if "%VAL%"=="" set "VAL=38"

reg add "HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl" /v "Win32PrioritySeparation" /t REG_DWORD /d %VAL% /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set Win32PrioritySeparation.
exit /b 1
