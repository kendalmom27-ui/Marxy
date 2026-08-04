@echo off
REM Disable GPU Preemption (Scheduler\EnablePreemption = 0)
REM
REM GPU preemption lets the scheduler interrupt a running GPU task to switch
REM to a higher-priority one. Disabling it (EnablePreemption = 0) makes the
REM GPU finish each task before switching, which some latency-focused setups
REM prefer for steadier frame delivery.
REM
REM HONEST NOTE: this is a community latency tweak, not an officially
REM documented Microsoft end-user setting. It's a real, settable key (verified
REM present on the test machine), but the tradeoff is genuine: without
REM preemption, frame pacing can actually get WORSE in some workloads, and
REM background GPU work can stutter the foreground. Treat it as experimental -
REM apply, test your games, and revert if it doesn't help.
REM
REM TO REVERT: set it back to 1 (preemption enabled) ->
REM   reg add "HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler" /v EnablePreemption /t REG_DWORD /d 1 /f

reg add "HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler" /v "EnablePreemption" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set EnablePreemption.
exit /b 1
