@echo off
REM Disable Fullscreen Optimizations (Global)
REM
REM Forces true exclusive fullscreen instead of Windows' borderless-style
REM "fullscreen optimizations" mode, which some games see lower input lag
REM and more consistent frame pacing from.
REM
REM HONEST NOTE: this used to be one clean global toggle, but Microsoft
REM has changed/broken this behavior inconsistently across Windows 10/11
REM updates since 1809. Community reports are mixed on whether the global
REM version reliably applies on current builds. If a specific game still
REM shows fullscreen optimizations behavior after running this, you may
REM need to also set it per-game via: right-click the game's .exe >
REM Properties > Compatibility > check "Disable fullscreen optimizations".

reg add "HKCU\System\GameConfigStore" /v "GameDVR_FSEBehaviorMode" /t REG_DWORD /d 2 /f
if errorlevel 1 goto :fail

reg add "HKCU\System\GameConfigStore" /v "GameDVR_HonorUserFSEBehaviorMode" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

reg add "HKCU\System\GameConfigStore" /v "GameDVR_DXGIHonorFSEWindowsCompatible" /t REG_DWORD /d 1 /f
if errorlevel 1 goto :fail

echo Done. Restart recommended for full effect.
exit /b 0

:fail
echo Failed to set fullscreen optimization registry values.
exit /b 1
