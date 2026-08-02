@echo off
REM Foreground Process Priority Boost (Win32PrioritySeparation)
REM
REM Controls how Windows splits CPU time between the active foreground
REM app and background processes. Value 0x26 (38 decimal) = short,
REM variable-length quanta with a high foreground boost - the most
REM commonly cited gaming value across performance guides, giving the
REM active window significantly more CPU attention.
REM
REM TRADEOFF: multitasking while gaming (streaming, background apps) can
REM feel less smooth since background processes get less CPU time.
REM A restart is required for this to take effect.

reg add "HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl" /v "Win32PrioritySeparation" /t REG_DWORD /d 38 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set Win32PrioritySeparation.
exit /b 1
