@echo off
REM Flatten Mouse Curve (disable acceleration)
REM
REM Replaces Windows' default mouse acceleration curve with a flat 1:1
REM response - a common competitive-gaming preference since acceleration
REM makes aim distance inconsistent between slow and fast movements.
REM This is a preference change, not a system-risk change, but it does
REM noticeably alter how the mouse feels, so it's flagged for confirmation.
REM
REM To revert: Control Panel > Mouse > Pointer Options > check
REM "Enhance pointer precision" (restores Windows defaults).

reg add "HKCU\Control Panel\Mouse" /v "MouseSpeed" /t REG_SZ /d "0" /f
if errorlevel 1 goto :fail

reg add "HKCU\Control Panel\Mouse" /v "MouseThreshold1" /t REG_SZ /d "0" /f
if errorlevel 1 goto :fail

reg add "HKCU\Control Panel\Mouse" /v "MouseThreshold2" /t REG_SZ /d "0" /f
if errorlevel 1 goto :fail

reg add "HKCU\Control Panel\Mouse" /v "SmoothMouseXCurve" /t REG_BINARY /d 0000000000000000c0cc0c0000000000809919000000000040662600000000000033330000000000 /f
if errorlevel 1 goto :fail

reg add "HKCU\Control Panel\Mouse" /v "SmoothMouseYCurve" /t REG_BINARY /d 0000000000000000000038000000000000007000000000000000a800000000000000e00000000000 /f
if errorlevel 1 goto :fail

echo Done. Sign out and back in (or replug mouse) for full effect.
exit /b 0

:fail
echo One or more commands failed.
exit /b 1
