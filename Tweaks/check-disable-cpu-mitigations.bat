@echo off
for /f "tokens=3" %%v in ('reg query "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v FeatureSettingsOverride 2^>nul ^| findstr FeatureSettingsOverride') do set VAL=%%v
if "%VAL%"=="0x3" (echo STATE:ON) else (echo STATE:OFF)
exit /b 0
