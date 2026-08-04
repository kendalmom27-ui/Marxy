@echo off
REM TrimProcessWorkingSet = 0 (Session Manager\Memory Management)
REM
REM Creates the TrimProcessWorkingSet DWORD and sets it to 0, the intent being
REM to stop Windows from trimming (paging out) process working sets.
REM
REM HONEST NOTE: this is NOT a documented Windows memory-manager value - it is
REM not part of the real Memory Management value set and doesn't exist by
REM default. Windows almost certainly ignores it, so treat this as a placebo
REM with no verifiable effect. Included by request.
REM
REM TO REVERT: delete the value ->
REM   reg delete "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v TrimProcessWorkingSet /f

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "TrimProcessWorkingSet" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

echo Done. Restart recommended.
exit /b 0

:fail
echo Failed to set TrimProcessWorkingSet.
exit /b 1
