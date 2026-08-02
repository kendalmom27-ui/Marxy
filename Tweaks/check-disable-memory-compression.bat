@echo off
for /f %%v in ('powershell -NoProfile -Command "(Get-MMAgent).MemoryCompression"') do set VAL=%%v
if /i "%VAL%"=="False" (echo STATE:ON) else (echo STATE:OFF)
exit /b 0
