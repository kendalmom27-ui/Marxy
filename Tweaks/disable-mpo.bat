@echo off
REM Disable Multiplane Overlay (MPO)
REM
REM MPO lets the GPU composite display layers (game, overlay, desktop)
REM as separate hardware planes instead of blending them through the
REM Desktop Window Manager. It's meant to save power/reduce latency, but
REM has a long, well-documented history of causing flickering, black
REM screens, and stutter on certain driver/GPU/monitor combinations -
REM especially with G-Sync/FreeSync and multi-monitor setups.
REM
REM IMPORTANT: this is a TARGETED FIX for a specific symptom, not a
REM general performance tweak. Only run this if you're actually seeing
REM flickering, black flashes (especially in Chrome/Edge), or stutter
REM tied to VRR. If your system runs cleanly, leave this alone - it can
REM slightly increase GPU compositing overhead for no benefit if you
REM don't have the problem it fixes.
REM
REM Undocumented by Microsoft (only appears in NVIDIA's own support
REM article, not Microsoft's docs) but extremely well-corroborated across
REM years of independent troubleshooting reports across NVIDIA, AMD, and
REM Intel hardware.
REM
REM Includes the Windows 11 24H2 companion fix (OverlayMinFPS=0), since
REM the primary OverlayTestMode value alone has been reported as
REM unreliable on 24H2 and newer.

reg add "HKLM\SOFTWARE\Microsoft\Windows\Dwm" /v "OverlayTestMode" /t REG_DWORD /d 5 /f
if errorlevel 1 goto :fail

reg add "HKLM\SOFTWARE\Microsoft\Windows\Dwm" /v "OverlayMinFPS" /t REG_DWORD /d 0 /f
if errorlevel 1 goto :fail

echo Done. Restart required for full effect.
exit /b 0

:fail
echo Failed to set MPO registry values.
exit /b 1
