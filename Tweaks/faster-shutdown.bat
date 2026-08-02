@echo off
REM Faster Shutdown
REM
REM Reduces how long Windows waits for unresponsive apps/services before
REM force-closing them during shutdown/restart/logoff. These are the four
REM standard, well-documented timeout/behavior values used for this:
REM
REM   - WaitToKillAppTimeout (HKCU): how long Windows waits for a hung app
REM     to respond before killing it. Default 5000-20000ms depending on
REM     Windows version, lowered here to 2000ms.
REM   - HungAppTimeout (HKCU): how long before an app is considered "not
REM     responding" in the first place. Default 5000ms, lowered to 2000ms.
REM   - AutoEndTasks (HKCU): tells Windows to automatically end hung tasks
REM     instead of showing a "This program isn't responding" prompt that
REM     blocks shutdown until a human clicks something.
REM   - WaitToKillServiceTimeout (HKLM): same idea, but for services rather
REM     than user apps. Default 5000-20000ms, lowered to 2000ms.
REM
REM TRADEOFF: an app that's just slow (not actually hung) has less time to
REM save its state before being force-closed. 2000ms is a common, widely
REM used middle ground - low enough to matter, high enough to rarely cut
REM off a genuinely-just-slow save.

reg add "HKCU\Control Panel\Desktop" /v "WaitToKillAppTimeout" /t REG_SZ /d "2000" /f
if errorlevel 1 goto :fail

reg add "HKCU\Control Panel\Desktop" /v "HungAppTimeout" /t REG_SZ /d "2000" /f
if errorlevel 1 goto :fail

reg add "HKCU\Control Panel\Desktop" /v "AutoEndTasks" /t REG_SZ /d "1" /f
if errorlevel 1 goto :fail

reg add "HKLM\SYSTEM\CurrentControlSet\Control" /v "WaitToKillServiceTimeout" /t REG_SZ /d "2000" /f
if errorlevel 1 goto :fail

echo Done. Sign out and back in for full effect.
exit /b 0

:fail
echo One or more commands failed.
exit /b 1
