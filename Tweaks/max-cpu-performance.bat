@echo off
REM Disable CPU Power Saving (Max Performance)
REM
REM Combines processor/power registry settings to reduce CPU power-saving
REM related latency:
REM   - Disables C-states (CPU idle power states)
REM   - Disables Windows Power Throttling (EcoQoS)
REM   - Unparks all CPU cores
REM   - Disables energy-estimation telemetry
REM   - Locks network adapter power management off at the policy level
REM
REM TRADEOFF: increases idle power draw and heat in exchange for more
REM consistent CPU response. Not recommended on laptops running on battery.
REM
REM NOTE: "Capabilities" and the two "PDC\Activators" VetoPolicy keys below
REM are undocumented by Microsoft (sourced from community tweak packs, not
REM official docs) - included because they're widely used without reported
REM issues, but their exact individual effect isn't independently verified
REM the way the rest of these are.

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Processor" /v "Cstates" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Processor" /v "Capabilities" /t REG_DWORD /d 0x7e066 /f
if errorlevel 1 goto :fail

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power" /v "HighPerformance" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power" /v "HighestPerformance" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power" /v "MinimumThrottlePercent" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power" /v "MaximumThrottlePercent" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power" /v "MaximumPerformancePercent" /t REG_DWORD /d 100 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power" /v "Class1InitialUnparkCount" /t REG_DWORD /d 100 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power" /v "InitialUnparkCount" /t REG_DWORD /d 100 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power" /v "EnergyEstimationEnabled" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\TaggedEnergy" /v "DisableTaggedEnergyLogging" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\TaggedEnergy" /v "TelemetryMaxApplication" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\TaggedEnergy" /v "TelemetryMaxTagPerApplication" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerThrottling" /v "PowerThrottlingOff" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\WcmSvc\GroupPolicy" /v "fDisablePowerManagement" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\PDC\Activators\Default\VetoPolicy" /v "EA:EnergySaverEngaged" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\PDC\Activators\28\VetoPolicy" /v "EA:PowerStateDischarging" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Misc" /v "DeviceIdlePolicy" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor" /v "PerfEnergyPreference" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor" /v "CPMinCores" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor" /v "CPMaxCores" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor" /v "CPMinCores1" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor" /v "CPMaxCores1" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor" /v "CpLatencyHintUnpark1" /t REG_DWORD /d 100 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor" /v "CPDistribution" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor" /v "CpLatencyHintUnpark" /t REG_DWORD /d 100 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor" /v "MaxPerformance1" /t REG_DWORD /d 100 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor" /v "MaxPerformance" /t REG_DWORD /d 100 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor" /v "CPDistribution1" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor" /v "CPHEADROOM" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor" /v "CPCONCURRENCY" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

echo Done. Restart recommended for full effect.
exit /b 0

:fail
echo One or more commands failed.
exit /b 1
