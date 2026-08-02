@echo off
for /f "tokens=3" %%v in ('reg query "HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate" /v ExcludeWUDriversInQualityUpdate 2^>nul ^| findstr ExcludeWUDriversInQualityUpdate') do set VAL=%%v
if "%VAL%"=="0x1" (echo STATE:ON) else (echo STATE:OFF)
exit /b 0
