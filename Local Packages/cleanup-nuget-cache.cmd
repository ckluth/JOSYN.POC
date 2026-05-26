@echo off
chcp 1252
setlocal

set NUGET_BASE=C:\Users\chris\.nuget\packages

:: ============================================================
:: Zu löschende Pakete hier eintragen 
:: ============================================================
set PACKAGES=^
  josyn.foundation.resultpattern ^
  josyn.foundation.propertybag ^
  josyn.foundation.jip ^
  josyn.system.shared.contract ^
  josyn.system.shared.log ^
  josyn.system.frontend.jobhost   

:: ============================================================

for %%P in (%PACKAGES%) do (
    if exist "%NUGET_BASE%\%%P" (
        echo Lösche: %NUGET_BASE%\%%P
        rd /s /q "%NUGET_BASE%\%%P"
        if errorlevel 1 (
            echo   FEHLER beim Löschen von %%P
        ) else (
            echo   OK
        )
    ) else (
        echo Nicht gefunden, übersprungen: %NUGET_BASE%\%%P
    )
)

echo.
echo Fertig.
endlocal

:: Pause nur wenn direkt aufgerufen (nicht per CALL aus einem anderen Skript)
if /i "%~1" neq "NOPAUSE" pause
