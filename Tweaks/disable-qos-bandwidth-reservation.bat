@echo off
REM Release QoS Bandwidth Reservation
REM
REM Windows' QoS Packet Scheduler reserves 20% of your connection's
REM bandwidth by default for apps that explicitly request guaranteed QoS
REM (this is the setting behind the old "20% bandwidth throttle" myth you've
REM probably seen repeated everywhere). This releases that reservation so
REM all bandwidth is available to normal best-effort traffic, which is what
REM the vast majority of UDP game traffic actually is.
REM
REM HONEST NOTE: very few apps actually request guaranteed QoS bandwidth in
REM practice, so the real-world impact of this specific setting is often
REM smaller than the myth suggests. It's still a real, documented,
REM zero-downside Microsoft policy key though - there's no reason to leave
REM bandwidth reserved for a feature almost nothing uses.

reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\Psched" /v "NonBestEffortLimit" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

echo Done. Restart recommended for full effect.
exit /b 0

:fail
echo Failed to set NonBestEffortLimit.
exit /b 1
