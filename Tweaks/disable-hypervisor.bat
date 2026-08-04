@echo off
REM Disable Hypervisor / VBS (bcdedit hypervisorlaunchtype off)
REM
REM Turns off the Windows hypervisor at boot, which also disables
REM Virtualization-Based Security (VBS) / Memory Integrity. Removes the small
REM CPU overhead those add, which some setups want for maximum gaming
REM performance.
REM
REM TRADEOFF: this turns OFF a security feature (VBS/Memory Integrity), and it
REM disables anything that relies on the hypervisor - WSL2, Windows Sandbox,
REM Hyper-V, and virtual machines will stop working until it's turned back on.
REM Restart required.
REM
REM TO REVERT: bcdedit /set hypervisorlaunchtype auto

bcdedit /set hypervisorlaunchtype off
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set hypervisorlaunchtype.
exit /b 1
