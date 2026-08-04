@echo off
REM Disable Working Set Trimming (TrimProcessWorkingSet = 0)
REM
REM Sets TrimProcessWorkingSet = 0 under Memory Management, to keep active
REM apps' memory resident instead of having their working sets trimmed/paged.
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
