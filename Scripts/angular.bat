@echo off
setlocal
cd /d "%~dp0..\Frontend"
echo.
echo [FollowMotors Angular] http://localhost:4200
echo [FollowMotors Angular] Peticiones /api hacia 127.0.0.1:5148
call npm run start
if errorlevel 1 (
  echo.
  echo [ERROR] Angular no pudo iniciar. Revisa los mensajes anteriores.
  pause
)
