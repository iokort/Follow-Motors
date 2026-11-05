# Follow Motors

Reconstrucción de la plataforma de gestión de clientes, unidades, kilometraje, servicios y garantías. La versión previa en [iokort.com/follow-motors](https://iokort.com/follow-motors) se mantiene como **referencia funcional** y no será sobrescrita automáticamente.

## Equipo y asignación

| Integrante | Encargado de | Rama de área |
| --- | --- | --- |
| Kort (@kortCobein) | **Backend Developer:** C# y ASP.NET Core .NET 10; integración técnica | `feature/backend` |
| Gael (@G43LEX) | **Database Manager:** MySQL y enfoque del producto | `feature/database` |
| Vixtor Fernando (@fernandoohiti25) | **Frontend Designer & Developer:** Angular 22, TypeScript, MVVM y CSS | `feature/frontend` |
| Aarón (@Aaron262005) | **Project Manager:** gestión y documentación | `feature/docs` |

## Stack definitivo acordado

- Backend: **.NET 10 LTS**, ASP.NET Core Web API, MySqlConnector 2.6.2.
- Base de datos: **MySQL 8.4**, contenedor local y almacenamiento persistente.
- **Frontend definitivo: Angular 22 + TypeScript con arquitectura MVVM** (plantillas HTML/CSS y estado reactivo con signals).
- Calidad: CI de .NET/MySQL y CodeQL; compilación Angular/TypeScript preparada en PR #15.
- Arquitectura: **MVVM en Angular** y **SOLID aplicado pragmáticamente en C#**; API REST bajo `/api/v1`.

**Estado:** `main` aún contiene el prototipo HTML/JS anterior, **no es el stack final**. Angular está implementado en la rama del [PR #15](https://github.com/iokort/Follow-Motors/pull/15), pendiente de revisión e integración. El [PR #14](https://github.com/iokort/Follow-Motors/pull/14) también requiere adaptación al stack acordado.

## Inicio rápido

Requisitos: Docker + Compose, Git. Opcional: .NET SDK 10 para desarrollar API fuera del contenedor.

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
| Frontend Angular (tras aprobar PR #15) | `cd frontend`, `npm install`, `npm start` → http://localhost:4200 |

Sin Docker, la API puede iniciarse con `dotnet run --project backend/FollowMotors.Api/FollowMotors.Api.csproj`. Configurar cadena de conexión mediante variable `ConnectionStrings__Default` si necesita acceder a MySQL externo a Compose.

## Estructura real

```text
backend/FollowMotors.Api/      Proyecto ASP.NET Core, salud y conexión MySQL
frontend/                      Prototipo temporal en main; Angular/MVVM propuesto en PR #15
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
npm run check
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

**Preparado en `main`:** API de salud, fábrica de conexión MySQL, Docker y frontend temporal.

**Preparado en PR #15:** proyecto Angular 22 con MVVM, pendiente de integrar.

**No implementado:** modelo de datos, autenticación, CRUD, clientes, unidades, servicios, agenda y garantías. Los cuatro responsables deben acordar esos requisitos. El sitio anterior continúa independiente.

No existe todavía una licencia explícita de reutilización.
