@echo off
CHCP 1252
setlocal

:: ================================================================
:: JOSYN — Build + Test All
:: Führt build-all.cmd und danach test-all.cmd aus.
:: ================================================================

call "%~dp0build-all.cmd"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo.
call "%~dp0test-all.cmd"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

pause

exit /b 0
