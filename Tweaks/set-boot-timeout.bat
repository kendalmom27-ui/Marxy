@echo off
REM Set Boot Menu Timeout to 6 Seconds
REM
REM Controls how long the Windows Boot Manager waits at the boot menu before
REM automatically starting the default OS. This is the same value exposed as
REM System Configuration (msconfig) > Boot > Timeout.
REM
REM Only has a visible effect if you actually have a boot menu (dual-boot,
REM or recovery entries). On a single-OS system the menu is usually skipped
REM entirely regardless of this value.
REM
REM Fully reversible: run "bcdedit /timeout 30" to restore the Windows
REM default, or set any other number of seconds you prefer.

bcdedit /timeout 6
if errorlevel 1 goto :fail

echo Done.
exit /b 0

:fail
echo Failed to set boot timeout.
exit /b 1
