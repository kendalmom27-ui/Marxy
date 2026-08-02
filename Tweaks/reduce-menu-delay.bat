@echo off
REM Reduce Menu Show Delay
REM
REM Controls how long Windows waits before showing a cascading submenu
REM when you hover over a menu item that has one - default is 400ms, a
REM holdover from Windows 95 meant to prevent accidental menu triggers
REM during fast mouse movement. Real, documented setting with zero GUI
REM exposure anywhere.
REM
REM HONEST NOTE: this only affects legacy Win32-style menus (File
REM Explorer right-click, Control Panel, Send To/New submenus) - it does
REM NOT affect Windows 11's modern WinUI Start Menu or flyouts. A recent
REM hands-on test found the real-world improvement modest, not the
REM dramatic "instant desktop" some guides claim - worth trying, but
REM don't expect a night-and-day difference.

reg add "HKCU\Control Panel\Desktop" /v "MenuShowDelay" /t REG_SZ /d "0" /f
if errorlevel 1 goto :fail

echo Done. Sign out and back in for full effect.
exit /b 0

:fail
echo Failed to set MenuShowDelay.
exit /b 1
