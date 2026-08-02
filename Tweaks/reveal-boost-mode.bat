@echo off
REM Reveal Processor Performance Boost Mode (PERFBOOSTMODE)
REM
REM This is a genuine CPU core-level scheduling policy that controls how
REM aggressively Windows requests turbo/boost behavior from your CPU.
REM Confirmed directly against Microsoft's hardware power-settings docs
REM (learn.microsoft.com/.../options-for-perf-state-engine-perfboostmode).
REM
REM Microsoft ships this setting with Attributes=1, which tells the Power
REM Options UI to hide it entirely - most users never know it exists. This
REM sets Attributes=2, which reveals it under:
REM Control Panel > Power Options > Change plan settings >
REM Change advanced power settings > Processor power management >
REM Processor performance boost mode
REM
REM This ONLY unhides the setting - it does not force any particular
REM boost mode. You choose the actual mode (Disabled/Enabled/Aggressive/
REM Efficient Aggressive/Rapid) yourself from that menu, since the "best"
REM choice genuinely depends on your CPU and cooling setup.

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\be337238-0d82-4146-a960-4f3749d470c7" /v "Attributes" /t REG_DWORD /d 2 /f
if errorlevel 1 goto :fail

echo Done. Check Power Options > Advanced settings > Processor power management.
exit /b 0

:fail
echo Failed to set Attributes value.
exit /b 1
