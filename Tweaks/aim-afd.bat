@echo off
REM AFD Network Tweak

reg add "HKLM\SYSTEM\CurrentControlSet\Services\AFD\Parameters" ^
/v FastSendDatagramThreshold ^
/t REG_DWORD ^
/d 64000 ^
/f >nul 2>&1

echo Done.
exit /b 0