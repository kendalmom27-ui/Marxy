@echo off
REM Disable NetBIOS over TCP/IP
REM
REM NetBIOS is a legacy name-resolution/browsing protocol from before DNS was
REM standard on local networks. Modern Windows doesn't need it, and leaving
REM it on means: extra broadcast chatter on the local network, and a legacy
REM protocol surface that's historically been a target for LAN-based exploits
REM (SMB/NetBIOS enumeration is a common first step in local network attacks).
REM
REM %~dp0 = the folder this .bat file lives in, so it finds the .ps1
REM regardless of what folder the app was launched from.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0disable-netbios.ps1"
if errorlevel 1 goto :fail

echo Done.
exit /b 0

:fail
echo Failed to update adapter registry keys.
exit /b 1
