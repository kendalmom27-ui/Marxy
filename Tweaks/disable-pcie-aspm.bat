@echo off
REM Disable PCIe Link State Power Management (ASPM)
REM
REM Every PCIe device (GPU, NVMe SSD, network card, etc.) can drop its
REM link into a low-power state (L1) when idle, then has to wake back up
REM (L0) when data needs to move again - a real, if small, latency cost
REM on every wake. This turns that off entirely so PCIe links stay fully
REM powered at all times. Confirmed against Microsoft's PCI Express power
REM settings documentation with exact GUIDs from multiple independent
REM sources, and specifically recommended in gaming-latency communities
REM (Blur Busters forums) alongside USB Selective Suspend.
REM
REM TRADEOFF: real increase in power draw and heat, since every PCIe
REM device runs at full power constantly instead of idling down. More
REM noticeable on laptops (battery life impact) than desktops.

set SUBGROUP=501a4d13-42af-4429-9fd1-a8218c268e20
set SETTING=ee12f906-d277-404b-b6da-e5fa1a576df5

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
echo Failed to set PCIe Link State Power Management.
exit /b 1
