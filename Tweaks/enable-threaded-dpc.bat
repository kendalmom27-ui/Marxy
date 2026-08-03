@echo off
REM Enable Threaded DPCs (ThreadDpcEnable)
REM
REM Controls whether Deferred Procedure Calls (DPCs) - the kernel-level work
REM drivers queue up to run after handling a hardware interrupt - execute in
REM normal interrupt context (DISPATCH_LEVEL, can't be preempted) or as
REM dedicated, schedulable kernel threads. This was introduced in Windows
REM Vista specifically to fix audio/input stutter caused by drivers with
REM long-running DPCs blocking everything else on that CPU core.
REM
REM HONEST NOTE: this is a real, genuinely settable value (confirmed against
REM this machine's registry), and a well-documented Vista/Windows 7-era
REM tuning option. Its real-world effect on Windows 10/11 specifically is
REM murkier - the NT kernel's DPC/interrupt handling has changed since then,
REM and unlike a registry value we can directly verify (like TdrDelay or
REM SystemResponsiveness), confirming an actual latency improvement from
REM this requires kernel-level DPC latency measurement tooling, not just a
REM before/after registry check. Widely repeated in tweak guides; treat the
REM performance claim with more skepticism than most other tweaks here.

reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel" /v "ThreadDpcEnable" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set ThreadDpcEnable.
exit /b 1
