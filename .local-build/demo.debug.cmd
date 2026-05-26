@echo off
CHCP 1252
setlocal

:: ================================================================
:: JOSYN — Demo (DEBUG)
:: 1. Baut JAPServer + MyDemoJob im Debug-Modus
:: 2. Startet beide in separaten Konsolfenstern
::
:: Debug-Build aktiviert #if DEBUG in JobHost.Core:
::   Console.WriteLine("[PRESS ANY KEY]");
::   Console.ReadKey(true);
::
:: JAPServer:  JOSYN-IPC dea5611d-d740-437f-ad93-7a5dc5ae4299
:: MyDemoJob:  JOSYN-IPC dea5611d-d740-437f-ad93-7a5dc5ae4299
:: ================================================================

set "ROOT=%~dp0.."
set "CONF=Debug"
set "ARGS=JOSYN-IPC dea5611d-d740-437f-ad93-7a5dc5ae4299"

set "BACKEND_SLNX=%ROOT%\JOSYN.System\JOSYN.System.Backend\JOSYN.System.Backend.slnx"
set "FRONTEND_SLNX=%ROOT%\JOSYN.System\JOSYN.System.Frontend\JOSYN.System.Frontend.slnx"

set "SERVER=C:\Temp\VS.OUT\JOSYN\JOSYN.System.Backend.JAPServer\bin\Debug\JOSYN.System.Backend.JAPServer.exe"
set "CLIENT=C:\Temp\VS.OUT\JOSYN\MyDemoJob\bin\Debug\JOSYN.MyDemoJob.exe"

echo.
echo ================================================================
echo  JOSYN Demo  [%CONF%]
echo ================================================================

:: --- Build Backend (JAPServer) -----------------------------------
echo.
echo [BUILD] JOSYN.System.Backend  [Debug]
dotnet build "%BACKEND_SLNX%" --configuration %CONF% --nologo
if %ERRORLEVEL% neq 0 (
    echo.
    echo [FEHLER] Backend-Build fehlgeschlagen.
    pause & exit /b 1
)

:: --- Build Frontend + MyDemoJob ----------------------------------
echo.
echo [BUILD] JOSYN.System.Frontend  [Debug]
dotnet build "%FRONTEND_SLNX%" --configuration %CONF% --nologo
if %ERRORLEVEL% neq 0 (
    echo.
    echo [FEHLER] Frontend-Build fehlgeschlagen.
    pause & exit /b 1
)

:: --- Verify exes exist -------------------------------------------
if not exist "%SERVER%" (
    echo [FEHLER] Server-Exe nicht gefunden nach Build:
    echo          %SERVER%
    pause & exit /b 1
)
if not exist "%CLIENT%" (
    echo [FEHLER] Client-Exe nicht gefunden nach Build:
    echo          %CLIENT%
    pause & exit /b 1
)

:: --- Starten -----------------------------------------------------
echo.
echo [START] JAPServer...
start "JAPServer [Debug]" "%SERVER%" %ARGS%

:: Kurz warten damit der Server die Pipes oeffnen kann
timeout /t 1 /nobreak >nul

echo [START] MyDemoJob...
start "MyDemoJob [Debug]" "%CLIENT%" %ARGS%

echo.
echo Beide Prozesse gestartet. [PRESS ANY KEY] wird im Job-Fenster erscheinen.
exit /b 0
