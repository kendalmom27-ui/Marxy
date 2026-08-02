@echo off
reg delete "HKLM\SOFTWARE\Microsoft\Windows\Windows Error Reporting" /v "Disabled" /f >nul 2>&1
sc config WerSvc start= demand
sc start WerSvc >nul 2>&1
echo Done.
exit /b 0
