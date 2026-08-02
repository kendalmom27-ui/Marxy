@echo off
REM Disable Windows Error Reporting
REM
REM Stops the background service that watches for app/system crashes and
REM sends diagnostic reports to Microsoft. Real, officially supported
REM setting - Microsoft ships a dedicated PowerShell module cmdlet for
REM this exact purpose (Disable-WindowsErrorReporting).
REM
REM Sets the registry key AND stops/disables the WerSvc service directly,
REM since the registry value alone can leave the service showing as
REM "running" even though it's not actually reporting (a known quirk
REM documented in Microsoft's own support forums).
REM
REM TRADEOFF: you lose local crash diagnostic reports (still visible in
REM Event Viewer either way, just not sent anywhere or shown as a popup).
REM Rare but real: some anti-cheat systems check that Windows services are
REM in their default state - if a specific game flags WerSvc as disabled,
REM re-enable it before playing that title.

reg add "HKLM\SOFTWARE\Microsoft\Windows\Windows Error Reporting" /v "Disabled" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

sc stop WerSvc >nul 2>&1
sc config WerSvc start= disabled
if errorlevel 1 goto :fail

echo Done.
exit /b 0

:fail
echo Failed to disable Windows Error Reporting.
exit /b 1
