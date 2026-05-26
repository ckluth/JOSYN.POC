@echo off
CHCP 1252
cd /d "%~dp0.."
dotnet pack JOSYN.Foundation.JIP --output "..\..\Local Packages"
REM pause
