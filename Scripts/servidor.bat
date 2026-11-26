@echo off
setlocal
cd /d "%~dp0.."
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=http://127.0.0.1:5148
echo.
echo [FollowMotors API] Entorno Development / Demo:Activado = true
echo [FollowMotors API] http://127.0.0.1:5148/api/estado
echo.
dotnet run --project "Backend\Presentation\FollowMotors.Presentation.csproj" --no-launch-profile
if errorlevel 1 (
  echo.
  echo [ERROR] La API no pudo iniciar. Revisa los mensajes de compilacion anteriores.
  pause
)
