# Follow Motors

Reconstrucción de la plataforma de gestión de clientes, unidades, kilometraje, servicios y garantías. La versión previa en [iokort.com/follow-motors](https://iokort.com/follow-motors) se mantiene como **referencia funcional** y no será sobrescrita automáticamente.

## Equipo y asignación

| Integrante | Encargado de | Rama de área |
| --- | --- | --- |
| Kort | Backend ASP.NET Core (C#) e integración con frontend; coordinación | `feature/backend` |
| Gael | Base de datos MySQL, modelo relacional y migraciones | `feature/database` |
| Víctor | Frontend HTML, CSS y JavaScript | `feature/frontend` |
| Aarón | Requisitos, casos de uso y documentación | `feature/docs` |

## Stack preparado

- Backend: **.NET 10 LTS**, ASP.NET Core Web API, MySqlConnector 2.6.2.
- Base de datos: **MySQL 8.4**, contenedor local y almacenamiento persistente.
- Frontend: HTML, CSS y JavaScript sin frameworks obligatorios.
- Calidad: GitHub Actions con compilación de API, verificación de MySQL, pruebas JS y análisis CodeQL; Dependabot semanal.
- Arquitectura: SOLID aplicado de forma pragmática, API REST bajo `/api/v1`.

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
| Frontend | Servir `frontend/` por HTTP en puerto 5500 (ej. Live Server) |

Sin Docker, la API puede iniciarse con `dotnet run --project backend/FollowMotors.Api/FollowMotors.Api.csproj`. Configurar cadena de conexión mediante variable `ConnectionStrings__Default` si necesita acceder a MySQL externo a Compose.

## Estructura real

```text
backend/FollowMotors.Api/      Proyecto ASP.NET Core, salud y conexión MySQL
frontend/                      HTML/CSS/JS inicial, cliente HTTP, pruebas
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
4. Antes de cambiar campos del frontend, endpoints o estructura MySQL, acordar el contrato entre Kort, Gael y Víctor; Aarón actualiza la documentación.
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

## Documentación

[Cómo colaborar](CONTRIBUTING.md) · [Arquitectura](docs/arquitectura.md) · [Contratos API](docs/api.md) · [Módulos anteriores](docs/modulos.md) · [Plan de trabajo](docs/plan-de-trabajo.md) · [Entornos y despliegue](docs/entornos-y-despliegue.md) · [Seguridad](SECURITY.md)

## Estado actual

**Preparado:** API de salud, fábrica de conexión MySQL, entorno Docker, estructura frontend y pruebas iniciales.

**No implementado:** modelo de datos, autenticación, CRUD, clientes, unidades, servicios, agenda y garantías. Los cuatro responsables deben acordar esos requisitos. El sitio anterior continúa independiente.

No existe todavía una licencia explícita de reutilización.
