@echo off
REM Increase Socket Buffer Size (AFD DefaultReceiveWindow/DefaultSendWindow)
REM
REM Windows' Ancillary Function Driver (afd.sys - the kernel layer Winsock
REM sits on top of) uses an internal default buffer size per socket when
REM these registry values aren't set (typically 8KB). Under bursty UDP
REM traffic - game netcode sending many packets in a short window, voice
REM chat, media streaming - a small buffer means packets can get dropped by
REM the OS before your app even reads them if it doesn't drain the socket
REM fast enough. Raising both to 64KB gives the OS more room to hold
REM incoming/outgoing data before that happens. Applies to UDP and TCP
REM sockets alike, since both sit on top of AFD.
REM
REM TRADEOFF: minor increase in per-socket kernel memory usage (a few extra
REM KB per open socket) - negligible on any modern system, but not literally
REM free.

reg add "HKLM\SYSTEM\CurrentControlSet\Services\AFD\Parameters" /v "DefaultReceiveWindow" /t REG_DWORD /d 65536 /f
if errorlevel 1 goto :fail

reg add "HKLM\SYSTEM\CurrentControlSet\Services\AFD\Parameters" /v "DefaultSendWindow" /t REG_DWORD /d 65536 /f
if errorlevel 1 goto :fail

echo Done. Restart recommended for full effect.
exit /b 0

:fail
echo Failed to set AFD buffer size values.
exit /b 1
