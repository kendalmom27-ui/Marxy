@echo off
REM Disable Memory Compression
REM
REM Windows compresses inactive memory pages instead of immediately
REM paging them to disk, trading CPU cycles (compression/decompression)
REM for effectively more usable RAM. Disabling it removes that CPU
REM overhead entirely, at the cost of Windows needing to page to disk
REM sooner under memory pressure.
REM
REM Real, documented PowerShell cmdlet (Disable-MMAgent -MemoryCompression),
REM part of the built-in Memory Manager Agent module.
REM
REM TRADEOFF: best suited to systems with plenty of RAM (16GB+). On
REM lower-RAM systems, this can make memory pressure situations worse
REM since Windows loses one of its ways to avoid disk paging.
REM
REM CONFLICT FIX: Disable-MMAgent internally needs to be able to start the
REM SysMain (Superfetch) service to query/set compression state. If SysMain
REM has been disabled (e.g. via the "Disable Superfetch" tweak), this fails
REM with Windows error 1058 ("the service cannot be started... disabled").
REM Confirmed by reproducing it directly. Fix: temporarily allow SysMain to
REM start, run the command, then restore whatever start type it had before -
REM so a prior "Disable Superfetch" choice is left untouched afterward.

set SYSMAIN_WAS_DISABLED=0
for /f "tokens=3" %%v in ('sc qc SysMain 2^>nul ^| findstr START_TYPE') do set SYSMAIN_START=%%v
if "%SYSMAIN_START%"=="4" (
    set SYSMAIN_WAS_DISABLED=1
    sc config SysMain start= demand >nul 2>&1
)

powershell -NoProfile -Command "Disable-MMAgent -MemoryCompression"
set MM_RESULT=%errorlevel%

if "%SYSMAIN_WAS_DISABLED%"=="1" (
    sc config SysMain start= disabled >nul 2>&1
)

if not "%MM_RESULT%"=="0" goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to disable memory compression.
exit /b 1
