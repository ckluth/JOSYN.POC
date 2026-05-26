@echo off
CHCP 1252
setlocal

:: ================================================================
:: JOSYN � Build All
:: 1. Leert den "Local Packages"-Ordner (*.nupkg)
:: 2. Bereinigt den globalen NuGet-Cache (alle JOSYN-Pakete)
:: 3. Baut + packt alle Sub-Repos in Abh�ngigkeitsreihenfolge
::    (jedes Paket wird sofort nach dem Build gepackt, damit
::     nachfolgende Sub-Repos es per NuGet-Restore finden)
:: Stoppt beim ersten Fehler.
:: ================================================================

set "ROOT=%~dp0.."
set "LOCALPKG=%ROOT%\Local Packages"
set "CONF=Release"

echo.
echo ================================================================
echo  JOSYN Build-All  [%CONF%]
echo  Crystal-clean: Local Packages + NuGet-Cache werden geleert
echo ================================================================

:: --- 1. Local Packages leeren -----------------------------------
echo.
echo [CLEAN] L�sche *.nupkg aus "%LOCALPKG%"
del /q "%LOCALPKG%\*.nupkg" 2>nul
echo        OK

:: --- 2. NuGet-Cache bereinigen ----------------------------------
echo.
echo [CLEAN] NuGet-Cache (JOSYN-Pakete)
call "%LOCALPKG%\cleanup-nuget-cache.cmd" NOPAUSE
echo.

:: --- 3. Build + Pack in Abh�ngigkeitsreihenfolge ---------------

call :build_and_pack ^
  "JOSYN.Foundation\JOSYN.Foundation.ResultPattern\JOSYN.Foundation.ResultPattern.slnx" ^
  "JOSYN.Foundation\JOSYN.Foundation.ResultPattern\JOSYN.Foundation.ResultPattern\JOSYN.Foundation.ResultPattern.csproj" ^
  "1/6  JOSYN.Foundation.ResultPattern"

call :build_and_pack ^
  "JOSYN.Foundation\JOSYN.Foundation.PropertyBag\JOSYN.Foundation.PropertyBag.slnx" ^
  "JOSYN.Foundation\JOSYN.Foundation.PropertyBag\JOSYN.Foundation.PropertyBag\JOSYN.Foundation.PropertyBag.csproj" ^
  "2/6  JOSYN.Foundation.PropertyBag"

call :build_and_pack ^
  "JOSYN.Foundation\JOSYN.Foundation.JIP\JOSYN.Foundation.JIP.slnx" ^
  "JOSYN.Foundation\JOSYN.Foundation.JIP\JOSYN.Foundation.JIP\JOSYN.Foundation.JIP.csproj" ^
  "3/6  JOSYN.Foundation.JIP"

call :build_and_pack ^
  "JOSYN.System\JOSYN.System.Shared\JOSYN.System.Shared.slnx" ^
  "JOSYN.System\JOSYN.System.Shared\JOSYN.System.Shared.Contract\JOSYN.System.Shared.Contract.csproj" ^
  "4/6  JOSYN.System.Shared.Contract"

dotnet pack "%ROOT%\JOSYN.System\JOSYN.System.Shared\JOSYN.System.Shared.Log\JOSYN.System.Shared.Log.csproj" --configuration %CONF% --no-build --output "%LOCALPKG%" --nologo
if %ERRORLEVEL% neq 0 ( echo. & echo [FEHLER] Pack: JOSYN.System.Shared.Log & exit /b 1 )
echo        JOSYN.System.Shared.Log packed.

call :build_and_pack ^
  "JOSYN.System\JOSYN.System.Frontend\JOSYN.System.Frontend.slnx" ^
  "JOSYN.System\JOSYN.System.Frontend\JOSYN.System.Frontend.JobHost\JOSYN.System.Frontend.JobHost.csproj" ^
  "5/6  JOSYN.System.Frontend.JobHost"

call :build_and_pack ^
  "JOSYN.System\JOSYN.System.Backend\JOSYN.System.Backend.slnx" ^
  "" ^
  "6/6  JOSYN.System.Backend  [exe � not packed]"

echo.
echo ================================================================
echo  [OK] Alle 6 Sub-Repos gebaut (7 NuGet-Pakete gepackt).
echo ================================================================
exit /b 0

:: ================================================================
:: Subroutine :build_and_pack  <slnx-rel>  <csproj-rel>  <label>
:: Pass empty string for <csproj-rel> to skip packing.
:: ================================================================
:build_and_pack
set "SLNX=%ROOT%\%~1"
set "CSPROJ=%~2"
set "LABEL=%~3"

echo.
echo [%LABEL%]
echo ----------------------------------------------------------------
dotnet build "%SLNX%" --configuration %CONF% --nologo
if %ERRORLEVEL% neq 0 ( echo. & echo [FEHLER] Build: %LABEL% & exit /b 1 )

if "%CSPROJ%"=="" exit /b 0

dotnet pack "%ROOT%\%CSPROJ%" --configuration %CONF% --no-build --output "%LOCALPKG%" --nologo
if %ERRORLEVEL% neq 0 ( echo. & echo [FEHLER] Pack:  %LABEL% & exit /b 1 )

exit /b 0

