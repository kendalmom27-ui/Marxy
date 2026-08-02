@echo off
REM Disable CPU Speculative Execution Mitigations (Spectre/Meltdown)
REM
REM Documented Microsoft registry toggle (KB4073119) that disables the
REM OS-level mitigations for Spectre Variant 2 (CVE-2017-5715) and
REM Meltdown (CVE-2017-5754). These mitigations have a real, measurable
REM CPU performance cost (branch prediction / kernel isolation overhead),
REM which is why disabling them is a genuine performance tweak - but it
REM is also a genuine security tradeoff, not just a "risk category".
REM
REM WHAT YOU LOSE: protection against real, published CPU side-channel
REM vulnerabilities that can allow malicious code to read memory it
REM shouldn't have access to across process/VM boundaries. This is the
REM same category of tradeoff as disabling Memory Integrity - it should
REM always be shown to the user with a clear warning before running,
REM never silently applied.

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "FeatureSettingsOverride" /t REG_DWORD /d 3 /f
if errorlevel 1 goto :fail

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "FeatureSettingsOverrideMask" /t REG_DWORD /d 3 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set CPU mitigation registry values.
exit /b 1
