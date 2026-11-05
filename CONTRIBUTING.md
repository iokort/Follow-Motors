# Guía de colaboración

## Responsables
- **Kort:** API ASP.NET Core, integración con frontend y coordinación técnica.
- **Gael:** MySQL, modelo relacional, migraciones y consultas.
- **Vixtor Fernando:** HTML, CSS, JavaScript, accesibilidad y componentes de interfaz.
- **Aarón:** requisitos, casos de uso, manuales y documentos del proyecto.

`main` = estable, `develop` = integración; las ramas `feature/backend`, `feature/database`, `feature/frontend`, `feature/docs` son áreas de referencia. Se recomiendan ramas por tarea (`feature/<descripcion>`, `fix/<descripcion>`, `docs/<descripcion>`) nacidas de `develop` cuando existan trabajos simultáneos.

## De Issue a PR
1. Elegir Issue y dejar definidos objetivo, alcance, dependencias y criterios de aceptación.
2. Obtener la versión más reciente de `develop` antes de iniciar una rama por tarea.
3. Modificar solo el área correspondiente. Pactar por adelantado cualquier cambio de API, estructura JSON o base de datos.
4. Probar lo modificado y documentar evidencias reales. No inventar pruebas.
5. Abrir PR contra `develop`, vincular el Issue y solicitar la revisión de otra persona.
6. Resolver conflictos y validaciones automáticas antes de integrar.
7. Para publicar: PR desde `develop` a `main`, revisión final y despliegue manual autorizado. Nunca desplegar una rama de trabajo directamente.

## Convenciones
Commits: `feat(api): ...`, `feat(db): ...`, `feat(frontend): ...`, `fix: ...`, `docs: ...`, `test: ...`, `chore: ...`.

Rutas: `/api/v1`. Contratos: solicitudes/respuestas JSON con códigos HTTP. El frontend solo consume la API; no debe conocer cadenas de conexión o SQL.

## Comprobaciones locales
- Backend: `dotnet build backend/FollowMotors.Api/FollowMotors.Api.csproj`.
- Frontend: `cd frontend && npm run check && npm test`.
- Integración local: `docker compose up -d --build` tras crear `.env`.
- CI ejecuta compilación, pruebas JS, salud de API y salud de MySQL.

## Restricciones
No subir credenciales, datos de clientes, respaldos o `.env`. No modificar producción desde `develop`, ni forzar pushes a ramas estables. No hacer migraciones destructivas sin plan de respaldo y aprobación expresa. No crear endpoints, tablas o funciones de negocio sin especificación.
