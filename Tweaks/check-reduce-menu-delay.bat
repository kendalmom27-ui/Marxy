@echo off
for /f "tokens=3" %%v in ('reg query "HKCU\Control Panel\Desktop" /v MenuShowDelay 2^>nul ^| findstr MenuShowDelay') do set VAL=%%v
if "%VAL%"=="0" (echo STATE:ON) else (echo STATE:OFF)
exit /b 0
