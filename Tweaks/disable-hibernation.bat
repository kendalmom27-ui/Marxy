@echo off
REM Disable Hibernation
REM
REM Frees up disk space (hiberfil.sys, often several GB) but also disables
REM Windows' "Fast Startup" feature, since Fast Startup relies on the
REM hibernation file. Boot will be a normal cold boot afterward instead.

powercfg -h off
if errorlevel 1 goto :fail

echo Done.
exit /b 0

:fail
echo Failed to disable hibernation.
exit /b 1
