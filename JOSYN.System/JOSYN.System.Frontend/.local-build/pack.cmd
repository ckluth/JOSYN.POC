@echo off
CHCP 1252
cd /d "%~dp0.."
dotnet pack JOSYN.System.Frontend.JobHost --output "..\..\Local Packages"
REM pause
