@echo off
REM Disable USB Selective Suspend
REM
REM Windows can power down individual USB ports/devices when they're
REM briefly idle to save power. For gaming mice and keyboards, this can
REM introduce a small wake-up delay the next time you move the mouse or
REM press a key - a real, documented cause of intermittent input lag.
REM Confirmed against Microsoft's own USB power-settings documentation
REM and corroborated across the Blur Busters input-latency community.
REM
REM TRADEOFF: minor increase in power draw, and on laptops running on
REM battery, slightly faster battery drain. No real downside on desktops.

set SUBGROUP=2a737441-1930-4402-8d77-b2bebba308a3
set SETTING=48e6b7a6-50f5-4782-a5d4-53bb8f07e226

powercfg -attributes %SUBGROUP% %SETTING% -ATTRIB_HIDE

powercfg /setacvalueindex SCHEME_CURRENT %SUBGROUP% %SETTING% 0
if errorlevel 1 goto :fail

powercfg /setdcvalueindex SCHEME_CURRENT %SUBGROUP% %SETTING% 0
if errorlevel 1 goto :fail

powercfg /setactive SCHEME_CURRENT
if errorlevel 1 goto :fail

echo Done.
exit /b 0

:fail
echo Failed to set USB Selective Suspend.
exit /b 1
