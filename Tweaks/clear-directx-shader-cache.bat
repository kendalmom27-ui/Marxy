@echo off
REM Clear DirectX Shader Cache
REM
REM Windows/DirectX caches compiled shaders under %LOCALAPPDATA%\D3DSCache
REM to skip recompilation on repeat launches. This cache can go stale or
REM corrupted - most commonly after a GPU driver update - causing visual
REM glitches or stutter. Clearing it is safe: Windows and your GPU driver
REM automatically rebuild it as needed.
REM
REM TRADEOFF: the first launch of each game/app after clearing will be
REM slightly slower while shaders recompile. One-time cost, not ongoing.
REM Some in-use files may fail to delete if a game is currently running -
REM that's normal and not a real failure.

rd /s /q "%LOCALAPPDATA%\D3DSCache" >nul 2>&1
md "%LOCALAPPDATA%\D3DSCache" >nul 2>&1

rd /s /q "%LOCALAPPDATA%\Microsoft\D3DSCache" >nul 2>&1

echo Done.
exit /b 0
