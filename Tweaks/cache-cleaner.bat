@echo off
REM Cache Cleaner
REM Clears temp files and Prefetch cache. Deliberately does NOT touch
REM Event Logs - wiping logs is an anti-forensic pattern, not a real
REM cleanup/performance action, and this app never does that.
REM
REM Note: some files will fail to delete because they're actively in use
REM (completely normal - Windows locks files that are open). That's not a
REM real failure, so this doesn't check errorlevel per-command like other
REM scripts do; it just reports overall completion.

del /f /s /q "%TEMP%\*" >nul 2>&1
rd /s /q "%TEMP%" >nul 2>&1
md "%TEMP%" >nul 2>&1

del /f /s /q "C:\Windows\Temp\*" >nul 2>&1

del /f /s /q "C:\Windows\Prefetch\*" >nul 2>&1

echo Done.
exit /b 0
