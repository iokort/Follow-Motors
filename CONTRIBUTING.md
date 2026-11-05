# Guía de colaboración

## Responsables
- **Aarón — Project Manager:** coordinación de entregas, tablero Scrum/Kanban, requisitos, documentación y evidencias.
- **Gael — Database Manager:** visión del producto, modelo relacional, migraciones, integridad y consultas MySQL.
- **Vixtor Fernando — Frontend Designer & Developer:** diseño UI, **Angular 22 con TypeScript**, componentes, plantillas HTML/CSS, accesibilidad y servicios HttpClient.
- **Kort — Backend Developer:** **C# / ASP.NET Core .NET 10**, API REST, reglas, validación, seguridad e integración técnica.

`main` = estable, `develop` = integración; las ramas `feature/backend`, `feature/database`, `feature/frontend`, `feature/docs` son áreas de referencia. Se recomiendan ramas por tarea (`feature/<descripcion>`, `fix/<descripcion>`, `docs/<descripcion>`) nacidas de `develop` cuando existan trabajos simultáneos.

## Stack definitivo

- **Frontend:** Angular 22, TypeScript y CSS. HTML es el lenguaje de las plantillas de Angular; no se desarrolla un sitio independiente en JavaScript sin framework.
- **Backend:** C# y ASP.NET Core (.NET 10).
- **Base de datos:** MySQL 8.4. El navegador no se conecta directamente a MySQL.
- **Alcance:** asistencia y posventa; los clientes y motos se dan de alta en sucursal por un asesor autorizado. Sin autorregistro ni venta pública.

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
- Frontend Angular: `cd frontend`, `npm install`, `npm run build`, `npm test` (Node.js compatible con Angular 22).
- Integración local: `docker compose up -d --build` tras crear `.env`.
- CI compila Angular/TypeScript, ejecuta pruebas de estructura y verifica la API .NET con MySQL. El check conserva temporalmente el identificador histórico `Frontend / JavaScript` para no romper las reglas de protección.

## Restricciones
No subir credenciales, datos de clientes, respaldos o `.env`. La lógica de autorización y pertenencia de motocicletas reside en ASP.NET Core, no en Angular. No modificar producción desde `develop`, ni forzar pushes a ramas estables. No hacer migraciones destructivas sin plan de respaldo y aprobación expresa. No crear endpoints, tablas o funciones de negocio sin especificación.
