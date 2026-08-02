@echo off
REM Visual Effects: Best Performance
REM Matches Windows' built-in "Adjust for best performance" option under
REM System Properties > Advanced > Performance Settings. Purely cosmetic -
REM no security or stability tradeoff, just less animation/transparency.

reg add "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects" /v "VisualFXSetting" /t REG_DWORD /d 3 /f
if errorlevel 1 goto :fail

reg add "HKCU\Control Panel\Desktop\WindowMetrics" /v "MinAnimate" /t REG_SZ /d "0" /f
if errorlevel 1 goto :fail

reg add "HKCU\Control Panel\Desktop" /v "DragFullWindows" /t REG_SZ /d "0" /f
if errorlevel 1 goto :fail

reg add "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v "TaskbarAnimations" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

reg add "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v "ListviewAlphaSelect" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

reg add "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v "ListviewShadow" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

echo Done. Sign out and back in for full effect.
exit /b 0

:fail
echo One or more commands failed.
exit /b 1
