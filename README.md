# Follow Motors

> **Asistencia y posventa:** el cliente se da de alta presencialmente por un asesor autorizado; no hay autorregistro ni venta en esta web. El portal privado solo muestra motocicletas vinculadas. [Alcance vigente](docs/alcance-posventa.md) · [ADR-0005](docs/adr/0005-alta-presencial-posventa.md).


Reconstrucción de la plataforma de gestión de clientes, unidades, kilometraje, servicios y garantías. La versión previa en [iokort.com/follow-motors](https://iokort.com/follow-motors) se mantiene como **referencia funcional** y no será sobrescrita automáticamente.

## Expediente académico y metodología

- **[Índice de entregables](docs/00-guia-academica.md)**: acceso único para evaluación.
- **[Scrum + Kanban](docs/scrum-kanban.md)**: sprints propuestos, Kanban, WIP, DoR y DoD.
- **[Product Backlog V5](docs/backlog-v5.md)**: 22 historias / 145 puntos preliminares.
- **[CRC extendidas](docs/crc/README.md)**: 180 fichas de análisis en US01–US08 y US21; [15 CRC originales](docs/tarjetas-crc.md).
- **[MVVM y arquitectura](docs/mvvm-y-arquitectura.md)**: capas web, C#, MySQL y SOLID.
- **[Marco teórico](docs/marco-teorico.md)**: fundamento con referencias.
- **[Stakeholders y RACI](docs/stakeholders-raci.md)** · **[Trazabilidad](docs/trazabilidad-y-evidencias.md)**.
- **[Kanban público SSC](https://github.com/orgs/iokort/projects/1)** · **[Discussions](https://github.com/iokort/Follow-Motors/discussions)**.

- **[Guía de GitHub: Wiki, Topics, Packages y Releases](docs/github-publicacion.md)**.

## Equipo y asignación

| Integrante | Encargado de | Rama de área |
| --- | --- | --- |
| Kort (@kortCobein) | Backend ASP.NET Core (C#) e integración con frontend; coordinación | `feature/backend` |
| Gael (@G43LEX) | Base de datos MySQL, modelo relacional y migraciones | `feature/database` |
| Vixtor Fernando (@fernandoohiti25) | Frontend Angular 22, TypeScript y CSS | `feature/frontend` |
| Aarón (@Aaron262005) | Requisitos, casos de uso y documentación | `feature/docs` |

## Stack preparado

- Backend: **.NET 10 LTS**, ASP.NET Core Web API, MySqlConnector 2.6.2.
- Base de datos: **MySQL 8.4**, contenedor local y almacenamiento persistente.
- Frontend: **Angular 22 y TypeScript**, componentes standalone, signals y HttpClient.
- Calidad: GitHub Actions con compilación de API, verificación de MySQL, compilación Angular y pruebas frontend, más análisis CodeQL; Dependabot semanal.
- Arquitectura: SOLID aplicado de forma pragmática, API REST bajo `/api/v1`.

## Inicio rápido

Requisitos: Docker + Compose y Git para backend/BD, Node.js 22.22.3+ o 24.15+ para Angular. Opcional: .NET SDK 10 para API fuera de Docker.

```bash
git clone https://github.com/iokort/Follow-Motors.git
cd Follow-Motors
git switch develop
cp .env.example .env
# Editar .env y reemplazar ambas contraseñas antes de ejecutar.
docker compose up -d --build
```

| Servicio local | Dirección |
| --- | --- |
| API | http://localhost:5099/api/v1/health |
| Salud MySQL, solo Development | http://localhost:5099/api/v1/health/database |
| MySQL | localhost:3307 (no accesible públicamente) |
| Frontend Angular | http://localhost:4200 después de `cd frontend && npm install && npm start` |

Sin Docker, la API puede iniciarse con `dotnet run --project backend/FollowMotors.Api/FollowMotors.Api.csproj`. Configurar cadena de conexión mediante variable `ConnectionStrings__Default` si necesita acceder a MySQL externo a Compose.

## Estructura real

```text
backend/FollowMotors.Api/      Proyecto ASP.NET Core, salud y conexión MySQL
frontend/                      Angular 22, TypeScript, estilos CSS y pruebas
database/                      Área de diseño de Gael; tablas todavía no definidas
docs/                          Arquitectura, API, requisitos y plan
.github/                       CI, CodeQL, Dependabot, PR/Issues, CODEOWNERS
compose.yaml                   Entorno de desarrollo API + MySQL
FollowMotors.slnx              Solución C# para IDE y línea de comandos
```

## Reglas de trabajo

1. **`main`** solo recibe versiones aprobadas; **`develop`** integra los cambios revisados.
2. Trabajar en ramas de área o por Issue (`feature/...`, `fix/...`, `docs/...`) nacidas de `develop`.
3. Para integrar, abrir PR a `develop`, asociar Issue y solicitar al menos una revisión. Describir pruebas y dependencias.
4. Antes de cambiar campos del frontend, endpoints o estructura MySQL, acordar el contrato entre Kort, Gael y Vixtor Fernando; Aarón actualiza la documentación.
5. No hacer force push ni integrar cambios sin CI; nadie trabaja directamente en `main`. Publicar requiere aprobación y rollback.
6. No subir `.env`, contraseñas, respaldos, datos personales ni código que conecte MySQL desde el navegador.
7. SOLID no significa crear capas innecesarias; cada dependencia debe justificarse.

**Nota:** GitHub permite asignar Issues y revisores; las ramas no tienen un dueño obligatorio. CODEOWNERS establece revisores de carpetas una vez verificados los nombres de usuario.

## Comprobaciones

```bash
dotnet build backend/FollowMotors.Api/FollowMotors.Api.csproj
cd frontend
npm install
npm run build
npm test
```

## Organización y colaboración

- [Roles y responsables](docs/equipo-y-roles.md)
- [Flujo Git y reglas](docs/flujo-de-trabajo.md)
- [Plan de trabajo por fases](docs/plan-de-trabajo.md)
- [Configuración administrativa desde Git CMD](scripts/configurar-equipo.cmd)

## Documentación

[Cómo colaborar](CONTRIBUTING.md) · [Arquitectura](docs/arquitectura.md) · [Contratos API](docs/api.md) · [Módulos anteriores](docs/modulos.md) · [Plan de trabajo](docs/plan-de-trabajo.md) · [Entornos y despliegue](docs/entornos-y-despliegue.md) · [Seguridad](SECURITY.md)

## Estado actual

**Preparado:** API de salud, fábrica de conexión MySQL, entorno Docker y aplicación Angular inicial con pruebas de estructura.

**No implementado:** modelo de datos, autenticación, CRUD, clientes, unidades, servicios, agenda y garantías. Los cuatro responsables deben acordar esos requisitos. El sitio anterior continúa independiente.

No existe todavía una licencia explícita de reutilización.

---

© 2026 Kurt Cobain Vázquez Sánchez · [ioKort](https://iokort.com)
