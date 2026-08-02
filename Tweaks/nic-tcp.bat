@echo off
REM NIC & TCP Tweaks
REM - Enables TCP auto-tuning (helps throughput on most connections)
REM - Enables RSS (Receive Side Scaling) so network load spreads across CPU cores
REM - Disables power-saving on all network adapters so Windows doesn't
REM   throttle/sleep the NIC to save battery (matters most on laptops)
REM
REM Exit code 0 = success, non-zero = failure. main.js reads this directly.

echo Setting TCP autotuning to normal...
netsh int tcp set global autotuninglevel=normal
if errorlevel 1 goto :fail

echo Enabling RSS (Receive Side Scaling)...
netsh int tcp set global rss=enabled
if errorlevel 1 goto :fail

echo Disabling NIC power-saving on all adapters...
powershell -NoProfile -Command "Disable-NetAdapterPowerManagement -Name '*' -Confirm:$false -ErrorAction Stop"
if errorlevel 1 goto :fail

echo Done.
exit /b 0

:fail
echo One or more commands failed - see output above.
exit /b 1
