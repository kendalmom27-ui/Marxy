@echo off
REM Revert CPU Security Mitigations
REM Per Microsoft's KB4073119, deleting the override values (rather than
REM setting them to 0) is the documented way to restore full default
REM protection - the values simply not existing means Windows uses its
REM normal mitigation state.
reg delete "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "FeatureSettingsOverride" /f >nul 2>&1
reg delete "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "FeatureSettingsOverrideMask" /f >nul 2>&1
echo Done. Restart required.
exit /b 0
