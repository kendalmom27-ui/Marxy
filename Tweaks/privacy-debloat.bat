@echo off
REM Privacy & Telemetry Debloat
REM Standard, well-documented telemetry/diagnostic opt-outs (the same kind
REM of settings exposed in Settings > Privacy, just consolidated). Does not
REM disable security features or Windows Update itself - only diagnostic
REM data collection and suggested-content features.

reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\CloudContent" /v "DisableWindowsConsumerFeatures" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

reg add "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" /v "SubscribedContent-338389Enabled" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

reg add "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Privacy" /v "TailoredExperiencesWithDiagnosticDataEnabled" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

reg add "HKLM\SYSTEM\CurrentControlSet\Control\WMI\AutoLogger\AutoLogger-Diagtrack-Listener" /v "Start" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

reg add "HKLM\SOFTWARE\Policies\Microsoft\SQMClient\Windows" /v "CEIPEnable" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\AppCompat" /v "AITEnable" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

echo Done.
exit /b 0

:fail
echo One or more commands failed.
exit /b 1
