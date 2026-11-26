@echo off
setlocal EnableExtensions
chcp 65001 >nul
cd /d "%~dp0"
title FollowMotors - Inicio de demostracion

echo.
echo  ===============================================
echo   FOLLOWMOTORS  ^|  Angular + API ASP.NET Core
echo  ===============================================
echo.
echo  Modo demostracion: datos ficticios en un archivo C#.
echo.

where dotnet >nul 2>&1
if errorlevel 1 (
  echo [ERROR] No se encontro dotnet. Instala el SDK de .NET 10.
  goto FIN_ERROR
)
where npm >nul 2>&1
if errorlevel 1 (
  echo [ERROR] No se encontro npm. Instala Node.js 22.12 o posterior.
  goto FIN_ERROR
)

dotnet --list-sdks | findstr /B /C:"10." >nul
if errorlevel 1 (
  echo [ERROR] Este proyecto necesita el SDK de .NET 10.
  echo SDK detectados:
  dotnet --list-sdks
  goto FIN_ERROR
)

if not exist "Frontend\node_modules\@angular\cli\bin\ng.js" (
  echo [1/3] Instalando dependencias del frontend (solo la primera vez)...
  pushd "Frontend"
  call npm install --no-audit --no-fund
  if errorlevel 1 (
    popd
    echo [ERROR] No fue posible instalar Angular. Revisa internet y npm.
    goto FIN_ERROR
  )
  popd
)

echo [2/3] Iniciando API C# en http://127.0.0.1:5148
start "FollowMotors - API" /D "%~dp0" cmd.exe /k call Scripts\servidor.bat

echo [3/3] Iniciando Angular en http://localhost:4200
start "FollowMotors - Angular" /D "%~dp0" cmd.exe /k call Scripts\angular.bat

echo.
echo  Se abrieron dos ventanas: API C# y Angular.
echo  La interfaz estara disponible en http://localhost:4200
start "" "http://localhost:4200"
echo.
echo  Al cerrar la ventana de la API se reiniciaran los datos de demostracion.
echo  Para comprobar los flujos automaticamente: probar-flujos.bat
exit /b 0

:FIN_ERROR
echo.
pause
exit /b 1
