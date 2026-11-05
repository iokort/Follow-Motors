# Guía de colaboración

## Responsables
- **Aarón — Project Manager:** coordinación del proyecto y documentación.
- **Gael — Database Manager:** MySQL y visión funcional.
- **Vixtor Fernando — Frontend Designer & Developer:** **Angular 22 con TypeScript y MVVM**, componentes y estilos HTML/CSS.
- **Kort — Backend Developer:** **ASP.NET Core .NET 10 (C#)** y coordinación de integración técnica.

## Tecnologías obligatorias

**Angular 22 + TypeScript + MVVM** para el frontend; **C# / .NET 10 + SOLID** para backend; **MySQL 8.4** para datos. HTML y CSS se emplean dentro de Angular, no como sustituto del framework.

**Situación:** en `main` queda un prototipo antiguo de JavaScript; la implementación Angular está en el [PR #15](https://github.com/iokort/Follow-Motors/pull/15), todavía no integrada. El PR #14 de frontend debe revisarse contra esta decisión.

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
- Frontend Angular tras integrar PR #15: en `frontend` ejecutar `npm install`, `npm run build` y `npm test`. En `main` aún permanece el prototipo anterior.
- Integración local: `docker compose up -d --build` tras crear `.env`.
- CI de `main` comprueba el prototipo actual; el PR #15 incorpora compilación Angular/TypeScript. La API se prueba con MySQL.

## Restricciones
No subir credenciales, datos de clientes, respaldos o `.env`. No modificar producción desde `develop`, ni forzar pushes a ramas estables. No hacer migraciones destructivas sin plan de respaldo y aprobación expresa. No crear endpoints, tablas o funciones de negocio sin especificación.
