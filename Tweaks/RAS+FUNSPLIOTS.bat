@echo off
REM Apply Custom Power Plan
REM
REM Imports the bundled custom-powerplan.pow file (exported earlier via
REM "powercfg -export") under a fixed GUID, then activates it. Using a
REM fixed GUID (instead of letting Windows generate a random one) means
REM running this again just re-imports/updates the same plan rather than
REM creating duplicates every time.
REM
REM This does NOT delete any other power plan - it only adds/updates this
REM one and switches to it.

set PLAN_GUID=7a3f9c10-4e2b-4a6d-9f1e-2c8b5d6a7f31
set PLAN_FILE=%~dp0RAS+FUNSPLIOTS.pow

REM powercfg -import refuses to overwrite an existing GUID, so check first
REM and only import if this plan hasn't been created yet. On repeat runs
REM we just rename/reactivate it instead of re-importing.
powercfg -query %PLAN_GUID% >nul 2>&1
if not errorlevel 1 goto :already_exists

powercfg -import "%PLAN_FILE%" %PLAN_GUID%
if errorlevel 1 goto :fail

:already_exists
powercfg -changename %PLAN_GUID% "RAS+Funspliots" "Custom imported power plan"
if errorlevel 1 goto :fail

powercfg -setactive %PLAN_GUID%
if errorlevel 1 goto :fail

echo Done.
exit /b 0

:fail
echo Failed to import or activate the custom power plan.
exit /b 1
