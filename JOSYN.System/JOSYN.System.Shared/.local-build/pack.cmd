@echo off
CHCP 1252
cd /d "%~dp0.."

dotnet pack JOSYN.System.Shared.Contract --output "..\..\Local Packages"
if %ERRORLEVEL% neq 0 (
    echo [FEHLER] Pack JOSYN.System.Shared.Contract fehlgeschlagen.
    exit /b %ERRORLEVEL%
)

dotnet pack JOSYN.System.Shared.Log --output "..\..\Local Packages"
if %ERRORLEVEL% neq 0 (
    echo [FEHLER] Pack JOSYN.System.Shared.Log fehlgeschlagen.
    exit /b %ERRORLEVEL%
)

echo.
echo [OK] Beide Pakete erfolgreich gepackt.
REM pause
