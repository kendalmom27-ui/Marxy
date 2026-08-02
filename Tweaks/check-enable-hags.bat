@echo off
for /f "tokens=3" %%v in ('reg query "HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers" /v HwSchMode 2^>nul ^| findstr HwSchMode') do set VAL=%%v
if "%VAL%"=="0x2" (echo STATE:ON) else (echo STATE:OFF)
exit /b 0
