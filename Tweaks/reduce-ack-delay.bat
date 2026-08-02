@echo off
REM Reduce TCP ACK Delay
REM
REM The original "Decrease Ping.bat" wrote these values to a registry path
REM that doesn't exist, so it did nothing. This calls a proper PowerShell
REM script (reduce-ack-delay.ps1, in this same folder) that loops through
REM every adapter's own registry subkey and sets:
REM   - TcpAckFrequency=1  -> don't delay ACKs (send them immediately)
REM   - TCPNoDelay=1       -> disable Nagle's algorithm
REM Together these reduce latency at the cost of slightly more packet
REM overhead - a real, standard competitive-gaming network tweak.
REM
REM %~dp0 = the folder this .bat file lives in, so it finds the .ps1
REM regardless of what folder the app was launched from.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0reduce-ack-delay.ps1"
if errorlevel 1 goto :fail

echo Done.
exit /b 0

:fail
echo Failed to update adapter registry keys.
exit /b 1
