@echo off
powershell -NoProfile -Command "Enable-MMAgent -MemoryCompression"
if errorlevel 1 goto :fail
echo Done. Restart required.
exit /b 0
:fail
echo Failed to re-enable memory compression.
exit /b 1
