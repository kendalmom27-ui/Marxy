@echo off
REM Exclude Driver Updates from Windows Update
REM Prevents Windows Update from automatically installing driver updates
REM (GPU, chipset, etc.) so you can manage those yourself directly from
REM Nvidia/AMD/Intel. Does not disable Windows Update itself - only the
REM driver-update category.

reg add "HKLM\SOFTWARE\Microsoft\PolicyManager\current\device\Update" /v "ExcludeWUDriversInQualityUpdate" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

reg add "HKLM\SOFTWARE\Microsoft\PolicyManager\default\Update" /v "ExcludeWUDriversInQualityUpdate" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate" /v "ExcludeWUDriversInQualityUpdate" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

echo Done.
exit /b 0

:fail
echo One or more commands failed.
exit /b 1
