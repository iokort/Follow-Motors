# Guía de colaboración

## Responsables
- **Aarón — Project Manager:** coordinación del proyecto y documentación.
- **Gael — Database Manager:** MySQL y visión funcional.
- **Vixtor Fernando — Frontend Designer & Developer:** **Angular 22 con TypeScript y MVVM**, componentes y estilos HTML/CSS.
- **Kort — Backend Developer:** **ASP.NET Core .NET 10 (C#)** y coordinación de integración técnica.

## Tecnologías obligatorias

**Angular 22 + TypeScript + MVVM** para el frontend; **C# / .NET 10 + SOLID** para backend; **MySQL 8.4** para datos. HTML y CSS se emplean dentro de Angular, no como sustituto del framework.

**Situación vigente:** `main` integra el diseño original de Fernando (PR #41) y Angular 22 + MVVM como contenedor/transición (PR #42). El código previo de vistas sigue siendo JavaScript hasta su migración incremental a Angular nativo. **No reemplazar** diseños del autor, ni considerar módulos completos por el simple hecho de compilar.

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
- Frontend Angular: en `frontend` ejecutar `npm install`, `npm run build` y `npm test`; el runtime conserva las pantallas originales de Fernando mientras se migra.
- Integración local: `docker compose up -d --build` tras crear `.env`.
- CI de `main` compila Angular/TypeScript con la interfaz original, además de API C# y MySQL. La cobertura de pruebas de negocio aún está pendiente.

## Restricciones
No subir credenciales, datos de clientes, respaldos o `.env`. No modificar producción desde `develop`, ni forzar pushes a ramas estables. No hacer migraciones destructivas sin plan de respaldo y aprobación expresa. No crear endpoints, tablas o funciones de negocio sin especificación.

## Organización Scrum y criterios

- [Cuatro épicas, 12 historias y 144 CRC dentro de sus US](docs/epicas/README.md).
- [Plan Scrum semanal propuesto y Definition of Done](docs/scrum-plan-ssc.md).
- [Reglas funcionales acordadas con Gael](docs/reglas-negocio-ssc.md).
- Mantener las historias en el tablero con estado real, nunca marcar una US como Done solo por completar una pantalla con mocks.
- Los cambios en `frontend/` deben preservar el diseño de Fernando; en `database/v3/`, conservar la estructura de Gael hasta acordar migraciones.
