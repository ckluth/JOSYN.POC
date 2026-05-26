@echo off
CHCP 1252
setlocal

:: ================================================================
:: JOSYN — Demo
:: Startet JAPServer und MyDemoJob mit den Hardcoded-Session-Args
:: aus deren launchSettings.json.
::
:: JAPServer:  JOSYN-IPC dea5611d-d740-437f-ad93-7a5dc5ae4299
:: MyDemoJob:  JOSYN-IPC dea5611d-d740-437f-ad93-7a5dc5ae4299
:: ================================================================

set "ARGS=JOSYN-IPC dea5611d-d740-437f-ad93-7a5dc5ae4299"
set "SERVER=C:\Temp\VS.OUT\JOSYN\JOSYN.System.Backend.JAPServer\bin\Release\JOSYN.System.Backend.JAPServer.exe"
set "CLIENT=C:\Temp\VS.OUT\JOSYN\MyDemoJob\bin\Release\JOSYN.MyDemoJob.exe"

if not exist "%SERVER%" (
    echo [FEHLER] Server-Exe nicht gefunden. Erst build-all ausfuehren.
    echo          %SERVER%
    pause & exit /b 1
)
if not exist "%CLIENT%" (
    echo [FEHLER] Client-Exe nicht gefunden. Erst build-all ausfuehren.
    echo          %CLIENT%
    pause & exit /b 1
)

echo Starte JAPServer...
start "JAPServer" "%SERVER%" %ARGS%

:: Kurz warten damit der Server die Pipes oeffnen kann
timeout /t 1 /nobreak >nul

echo Starte MyDemoJob...
start "MyDemoJob" "%CLIENT%" %ARGS%

exit /b 0
