@echo off
REM Flush DNS Cache
echo Flushing DNS cache...
ipconfig /flushdns
if errorlevel 1 goto :fail

echo Done.
exit /b 0

:fail
echo Failed to flush DNS cache.
exit /b 1
