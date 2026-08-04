@echo off
REM Disable GPU Energy Driver (GpuEnergyDrv)
REM
REM Sets the GpuEnergyDrv kernel service's Start type to 4 (Disabled), which
REM stops that driver from loading on the next boot. It's a GPU power/energy
REM management helper driver; disabling it is a community tweak circulated in
REM NVIDIA tuning guides, hence its placement under the NVIDIA vendor tab.
REM
REM NOTE: the service is named "GpuEnergyDrv" (no space) - some guides write
REM it as "GPU EnergyDrv" with a space, which points at a key that doesn't
REM exist and silently does nothing. This uses the correct name.
REM
REM HONEST NOTE: this could not be verified on real NVIDIA hardware (the test
REM machine has an Intel iGPU), and this service actually exists on non-NVIDIA
REM systems too - so it is not strictly NVIDIA-exclusive. Its real-world effect
REM is not independently confirmed the way the documented registry tweaks in
REM this app are. Treat the benefit claim with skepticism.
REM
REM TO REVERT: set Start back to 3 (Manual) - the usual default for this
REM service:  reg add "HKLM\SYSTEM\CurrentControlSet\Services\GpuEnergyDrv" /v Start /t REG_DWORD /d 3 /f

reg add "HKLM\SYSTEM\CurrentControlSet\Services\GpuEnergyDrv" /v "Start" /t REG_DWORD /d 4 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set GpuEnergyDrv Start value.
exit /b 1
