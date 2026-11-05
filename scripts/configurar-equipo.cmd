@echo off
setlocal EnableExtensions
rem Ejecutar desde la raiz del repositorio: call scripts\configurar-equipo.cmd
rem Requiere GitHub CLI autenticado como administrador.
set "REPO=iokort/Follow-Motors"
echo.
echo === FOLLOW MOTORS: CONFIGURACION COLABORATIVA ===
where gh >nul 2>nul
if errorlevel 1 (
  echo ERROR: falta GitHub CLI. Instala con winget install --id GitHub.cli -e
  exit /b 1
)
gh auth status
if errorlevel 1 (
  echo ERROR: inicia sesion con gh auth login
  exit /b 1
)
if not exist "scripts\branch-protection.json" (
  echo ERROR: ejecuta este script desde la raiz del repositorio.
  exit /b 1
)
echo.
echo [1/6] Verificando permisos actuales...
gh repo view %REPO% --json viewerPermission --jq ".viewerPermission"
gh api repos/%REPO%/collaborators/G43LEX/permission --jq ".permission"
gh api repos/%REPO%/collaborators/fernandoohiti25/permission --jq ".permission"
gh api repos/%REPO%/collaborators/Aaron262005/permission --jq ".permission"
echo.
echo [2/6] Organizando etiquetas...
gh label create "area:backend" -R %REPO% --color "00245A" --description "C# ASP.NET API" --force
gh label create "area:database" -R %REPO% --color "009D81" --description "MySQL y migraciones" --force
gh label create "area:frontend" -R %REPO% --color "2C87CA" --description "HTML CSS JavaScript" --force
gh label create "area:docs" -R %REPO% --color "B6A5D6" --description "Requisitos y documentacion" --force
gh label create "area:integration" -R %REPO% --color "F1B84A" --description "Integracion y despliegue" --force
gh label create "status:blocked" -R %REPO% --color "DA3633" --description "Pendiente de una dependencia" --force
echo.
echo [3/7] Creando hitos de seguimiento (sin duplicar)...
gh api repos/%REPO%/milestones --jq ".[].title" | findstr /C:"F0 - Gobierno" >nul
if errorlevel 1 gh api --method POST repos/%REPO%/milestones -f title="F0 - Gobierno" -f description="Colaboradores, accesos, entorno, reglas y CI" --jq ".number"
gh api repos/%REPO%/milestones --jq ".[].title" | findstr /C:"F1 - Contratos" >nul
if errorlevel 1 gh api --method POST repos/%REPO%/milestones -f title="F1 - Contratos" -f description="Requisitos, interfaces, esquema MySQL y contratos API" --jq ".number"
gh api repos/%REPO%/milestones --jq ".[].title" | findstr /C:"F2 - Primer flujo" >nul
if errorlevel 1 gh api --method POST repos/%REPO%/milestones -f title="F2 - Primer flujo" -f description="Primer flujo completo frontend API MySQL documentado y validado" --jq ".number"
gh api repos/%REPO%/milestones --jq ".[].title" | findstr /C:"F3 - Modulos" >nul
if errorlevel 1 gh api --method POST repos/%REPO%/milestones -f title="F3 - Modulos" -f description="Clientes, unidades, kilometraje, agenda, servicios y garantias" --jq ".number"
gh api repos/%REPO%/milestones --jq ".[].title" | findstr /C:"F4 - Entrega" >nul
if errorlevel 1 gh api --method POST repos/%REPO%/milestones -f title="F4 - Entrega" -f description="Pruebas, seguridad, despliegue manual y rollback" --jq ".number"
echo.
echo [4/7] Asignando actividades existentes...
gh issue edit 1 -R %REPO% --add-assignee kortCobein --add-label "area:backend" --milestone "F1 - Contratos"
gh issue edit 2 -R %REPO% --add-assignee G43LEX --add-label "area:database" --milestone "F1 - Contratos"
gh issue edit 3 -R %REPO% --add-assignee fernandoohiti25 --add-label "area:frontend" --milestone "F1 - Contratos"
gh issue edit 4 -R %REPO% --add-assignee Aaron262005 --add-label "area:docs" --milestone "F1 - Contratos"
gh issue edit 5 -R %REPO% --add-assignee kortCobein --add-label "area:integration" --milestone "F4 - Entrega"
echo.
echo [5/7] Configurando equipo GitHub (no se alteran privilegios admin)...
gh api orgs/iokort/teams/follow-motors --silent >nul 2>nul
if errorlevel 1 (
  gh api --method POST orgs/iokort/teams -f name="Follow Motors" -f description="Equipo de desarrollo y documentacion de Follow Motors" -f privacy=closed -f permission=push --jq ".slug"
)
gh api --method PUT orgs/iokort/teams/follow-motors/repos/iokort/Follow-Motors -f permission=push
gh api --method PUT orgs/iokort/teams/follow-motors/memberships/kortCobein -f role=maintainer --jq ".state"
gh api --method PUT orgs/iokort/teams/follow-motors/memberships/fernandoohiti25 -f role=member --jq ".state"
gh api --method PUT orgs/iokort/teams/follow-motors/memberships/Aaron262005 -f role=member --jq ".state"
rem Si Gael no ha aceptado la invitacion, la membresia puede quedar pending.
gh api --method PUT orgs/iokort/teams/follow-motors/memberships/G43LEX -f role=member --jq ".state"
echo.
echo [6/7] Estableciendo reglas main/develop...
gh api --method PUT repos/%REPO%/branches/main/protection --input "scripts\branch-protection.json" --jq ".required_pull_request_reviews.required_approving_review_count"
if errorlevel 1 echo AVISO: no se aplico proteccion a main. Revisa permisos de administracion.
gh api --method PUT repos/%REPO%/branches/develop/protection --input "scripts\branch-protection.json" --jq ".required_pull_request_reviews.required_approving_review_count"
if errorlevel 1 echo AVISO: no se aplico proteccion a develop. Revisa permisos de administracion.
echo.
echo [7/7] Verificando resultados...
gh issue list -R %REPO% --limit 20 --json number,title,assignees --jq ".[] | {issue:.number, responsables:[.assignees[].login]}"
gh api orgs/iokort/teams/follow-motors/members --jq ".[].login"
gh api orgs/iokort/teams/follow-motors/invitations --jq ".[].login"
gh api repos/%REPO%/branches/main --jq "{branch:.name, protected:.protected}"
gh api repos/%REPO%/branches/develop --jq "{branch:.name, protected:.protected}"
echo.
echo LISTO. Comprueba cualquier ERROR o AVISO antes de continuar.
echo Para tablero de GitHub Projects: gh auth refresh -s project ^& gh project create --owner iokort --title "Follow Motors - Desarrollo"
echo El sitio de produccion no fue modificado.
endlocal
