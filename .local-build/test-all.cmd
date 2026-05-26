@echo off
CHCP 1252
setlocal

:: ================================================================
:: JOSYN — Test All
:: Führt alle Tests in allen Sub-Repos aus (Release).
:: Stoppt beim ersten Fehler.
:: ================================================================

set "ROOT=%~dp0.."
set "CONF=Release"

echo.
echo ================================================================
echo  JOSYN Test-All  [%CONF%]
echo ================================================================

echo.
echo [1/3] JOSYN.Foundation.ResultPattern  ^(113 Tests^)
echo ----------------------------------------------------------------
dotnet test "%ROOT%\JOSYN.Foundation\JOSYN.Foundation.ResultPattern\JOSYN.Foundation.ResultPattern.slnx" --configuration %CONF% --nologo
if %ERRORLEVEL% neq 0 ( echo. & echo [FEHLER] JOSYN.Foundation.ResultPattern & exit /b 1 )

echo.
echo [2/3] JOSYN.Foundation.PropertyBag  ^(47 Tests^)
echo ----------------------------------------------------------------
dotnet test "%ROOT%\JOSYN.Foundation\JOSYN.Foundation.PropertyBag\JOSYN.Foundation.PropertyBag.slnx" --configuration %CONF% --nologo
if %ERRORLEVEL% neq 0 ( echo. & echo [FEHLER] JOSYN.Foundation.PropertyBag & exit /b 1 )

echo.
echo [3/3] JOSYN.Foundation.JIP  ^(1 Test^)
echo ----------------------------------------------------------------
dotnet test "%ROOT%\JOSYN.Foundation\JOSYN.Foundation.JIP\JOSYN.Foundation.JIP.slnx" --configuration %CONF% --nologo
if %ERRORLEVEL% neq 0 ( echo. & echo [FEHLER] JOSYN.Foundation.JIP & exit /b 1 )

echo.
echo ================================================================
echo  [OK] Alle 161 Tests bestanden.
echo ================================================================
exit /b 0
