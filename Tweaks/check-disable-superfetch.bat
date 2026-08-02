@echo off
for /f "tokens=3" %%v in ('sc qc SysMain 2^>nul ^| findstr START_TYPE') do set VAL=%%v
echo %VAL% | findstr /i "DISABLED" >nul
if %errorlevel%==0 (echo STATE:ON) else (echo STATE:OFF)
exit /b 0
