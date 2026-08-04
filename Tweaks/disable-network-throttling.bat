@echo off
REM Disable Network Throttling (NetworkThrottlingIndex = 0xFFFFFFFF)
REM
REM Windows' Multimedia Class Scheduler throttles non-multimedia network
REM traffic to ~10 packets/ms while multimedia (audio/video) is playing, to
REM protect playback smoothness. Setting NetworkThrottlingIndex to 0xFFFFFFFF
REM disables that throttling entirely, letting network traffic run unrestricted.
REM Commonly recommended for lower online-game latency. Documented, widely used.
REM
REM TRADEOFF: the throttle exists to keep audio/video playback smooth under
REM heavy network load - with it off, media playback can stutter more on some
REM systems while the network is saturated. Restart required.
REM
REM TO REVERT: set it back to Windows' default of 10 (0xa) ->
REM   reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v NetworkThrottlingIndex /t REG_DWORD /d 10 /f

reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v "NetworkThrottlingIndex" /t REG_DWORD /d 0xffffffff /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set NetworkThrottlingIndex.
exit /b 1
