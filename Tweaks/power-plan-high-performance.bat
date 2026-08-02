@echo off
REM High Performance Power Plan
REM
REM The original script DELETED all 3 built-in power plans (Balanced, High
REM Performance, Power Saver) - that's destructive and mostly irreversible
REM without running "powercfg -restoredefaultschemes". Switching to the
REM existing High Performance plan gets the same practical benefit without
REM the risk.

set HIGH_PERF_GUID=8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c

powercfg /setactive %HIGH_PERF_GUID%
if errorlevel 1 (
  REM Plan not visible/available on this system - create it from the
  REM hidden built-in template, then activate it.
  powercfg -duplicatescheme %HIGH_PERF_GUID%
  powercfg /setactive %HIGH_PERF_GUID%
  if errorlevel 1 goto :fail
)

echo Done.
exit /b 0

:fail
echo Failed to switch power plan.
exit /b 1
