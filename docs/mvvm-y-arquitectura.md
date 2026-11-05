# Arquitectura — Angular, ASP.NET Core y MySQL

El stack definitivo es **Angular 22 + TypeScript**, **ASP.NET Core .NET 10 / C#** y **MySQL 8.4**.

## Arquitectura

```mermaid
flowchart TB
  subgraph FE["Angular 22"]
    V["Plantillas y componentes (View)"]
    VM["HealthViewModel · estado y acciones (MVVM)"]
    HTTP["Servicio HttpClient"]
    V <--> VM
    VM --> HTTP
  end
  HTTP -->|"HTTPS /api/v1 JSON"| API["ASP.NET Core / C#"]
  API --> CASE["Casos de uso / contratos"]
  CASE --> DOM["Entidades y reglas de dominio"]
  CASE --> REPO["Adaptadores MySQL"]
  REPO --> DB[("MySQL 8.4")]
```

La arquitectura de presentación elegida es **MVVM (Model–View–ViewModel)**. En Angular, la plantilla representa la View, un servicio de presentación independiente concentra el estado y las acciones del ViewModel y los servicios HTTP y DTO conforman el acceso a datos del Model. Se implementó esta separación en la pantalla inicial de conectividad. Angular no accede a MySQL ni decide autorización; se crearán ViewModels independientes cuando la pantalla necesite estado o lógica de presentación, sin duplicar clases innecesariamente.

## Ejemplos reales de la base

- View: `frontend/src/app/app.html` y estilos `src/styles.css`.
- ViewModel: `frontend/src/app/viewmodels/health.viewmodel.ts` (signal de conectividad y acción de comprobación).
- Componente: `frontend/src/app/app.ts` (inyecta el ViewModel y coordina el ciclo de vida).
- API: `frontend/src/app/services/health.service.ts`.
- Backend: `backend/FollowMotors.Api/`.
- Desarrollo: proxy `frontend/proxy.conf.json` para `/api`.

## Políticas y SOLID

Los componentes y los servicios conservan responsabilidades diferenciadas, y los contratos TypeScript representan DTO HTTP. La API C# valida sesión, permisos y pertenencia de motos, mediante reglas de negocio y repositorios detrás de interfaces cuando aportan valor.

**Decisión comercial:** portal de asistencia/posventa, catálogo público solo informativo, alta presencial por asesor autorizado y portal privado para motos propias. [ADR-0005](adr/0005-alta-presencial-posventa.md).

## Estado de implementación

Existe una pantalla técnica Angular **separada mediante MVVM** y comprobación de salud de la API. El esquema de negocio MySQL, login real, alta presencial, agenda, historial y garantías **no se consideran implementados**. El catálogo CRC es diseño propuesto, no clases ya desarrolladas.

## Referencias

Angular. (2026). *Version compatibility*. https://angular.dev/reference/versions

Angular. (s. f.). *Building Angular apps*. https://angular.dev/tools/cli/build

OWASP Foundation. (s. f.). *Authorization cheat sheet*. https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html
