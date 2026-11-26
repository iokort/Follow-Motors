@echo off
chcp 65001 >nul
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Scripts\ProbarFlujos.ps1"
if errorlevel 1 (
  echo.
  echo Una o mas pruebas no se completaron. Consulta los errores anteriores.
)
pause
