@echo off
REM Disable AHCI Link Power Management (HIPM/DIPM)
REM
REM This is the real, single mechanism behind what's often marketed as
REM separate "DIPM Parking" / "HIPM Parking" / "SSD Powersaving" tweaks -
REM it's actually one setting with different power modes, not several
REM independent toggles. Confirmed against Microsoft's own official docs
REM (learn.microsoft.com/.../disk-settings-link-power-management-mode).
REM
REM The SATA AHCI controller can put the link to your HDD/SSD into a very
REM low power state during idle gaps (this is what causes HDD "head
REM parking" - the drive parks its heads as part of entering that low
REM power state). Setting this to "Active" disables that behavior
REM entirely, keeping the link at full power at all times.
REM
REM Hidden by default in Power Options (Microsoft ships it with
REM ATTRIB_HIDE) - this unhides it and sets it to Active.
REM
REM TRADEOFF: real increase in power draw and heat from your storage
REM devices, since the SATA link never drops to a low-power state. On an
REM HDD specifically, this also means it won't get the brief periodic
REM idle window Windows normally gives drives for internal maintenance.

set SUBGROUP=0012ee47-9041-4b5d-9b77-535fba8b1442
set SETTING=0b2d69d7-a2a1-449c-9680-f91c70521c60

powercfg -attributes %SUBGROUP% %SETTING% -ATTRIB_HIDE

powercfg /setacvalueindex SCHEME_CURRENT %SUBGROUP% %SETTING% 0
if errorlevel 1 goto :fail

powercfg /setdcvalueindex SCHEME_CURRENT %SUBGROUP% %SETTING% 0
if errorlevel 1 goto :fail

powercfg /setactive SCHEME_CURRENT
if errorlevel 1 goto :fail

echo Done.
exit /b 0

:fail
echo Failed to set AHCI Link Power Management.
exit /b 1
