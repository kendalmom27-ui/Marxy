@echo off
REM Set System Cooling Policy to Active
REM
REM Controls how the kernel's thermal management responds when the CPU
REM gets hot: Active mode increases fan speed BEFORE reducing CPU clock
REM speed; Passive mode does the opposite (throttles first, fans later).
REM Windows often defaults to Passive on laptops to save battery/noise.
REM Confirmed against Microsoft's power-settings GUID documentation and
REM consistent across many independent sources.
REM
REM TRADEOFF: more fan noise, and on laptops running on battery, faster
REM battery drain (since the CPU keeps running at full speed longer
REM instead of throttling down). Desktop users have no real downside
REM here since fan noise/power draw matter less than sustained performance.
REM
REM Note: some laptops with manufacturer-specific thermal software (Dell
REM Power Manager, etc.) may override this and ignore Windows' setting.

powercfg /setacvalueindex SCHEME_CURRENT SUB_PROCESSOR SYSCOOLPOL 1
if errorlevel 1 goto :fail

powercfg /setdcvalueindex SCHEME_CURRENT SUB_PROCESSOR SYSCOOLPOL 1
if errorlevel 1 goto :fail

powercfg /setactive SCHEME_CURRENT
if errorlevel 1 goto :fail

echo Done.
exit /b 0

:fail
echo Failed to set System Cooling Policy.
exit /b 1
