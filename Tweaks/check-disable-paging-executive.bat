@echo off
for /f "tokens=3" %%v in ('reg query "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v DisablePagingExecutive 2^>nul ^| findstr DisablePagingExecutive') do set VAL=%%v
if "%VAL%"=="0x1" (echo STATE:ON) else (echo STATE:OFF)
exit /b 0
