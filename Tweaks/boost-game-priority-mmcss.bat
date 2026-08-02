@echo off
REM Boost Game Thread Priority (MMCSS)
REM
REM Windows' Multimedia Class Scheduler Service (MMCSS) assigns CPU/GPU
REM scheduling priority based on task categories that apps register their
REM threads under (via the AvSetMmThreadCharacteristics API) - "Games",
REM "Audio", "Pro Audio", etc. This raises the "Games" category's GPU
REM Priority from its stock default (8) to 18, and its Scheduling
REM Category from the stock default (Medium) to High - giving threads
REM tagged as "Games" meaningfully more CPU/GPU scheduling priority
REM relative to other tasks. Real, documented mechanism (the Games task
REM category itself is part of Microsoft's MMCSS API), though this
REM specific elevated value combination is a widely-circulated community
REM tweak rather than an official Microsoft recommendation.
REM
REM Also includes IRQ8Priority=1, a small real-time clock interrupt
REM priority tweak commonly paired with this one.

reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "GPU Priority" /t REG_DWORD /d 18 /f
if errorlevel 1 goto :fail

reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "Priority" /t REG_DWORD /d 2 /f
if errorlevel 1 goto :fail

reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "Scheduling Category" /t REG_SZ /d "High" /f
if errorlevel 1 goto :fail

reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "SFIO Priority" /t REG_SZ /d "High" /f
if errorlevel 1 goto :fail

reg add "HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl" /v "IRQ8Priority" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

echo Done. Restart recommended for full effect.
exit /b 0

:fail
echo Failed to set MMCSS Games task priority.
exit /b 1
