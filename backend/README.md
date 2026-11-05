# Backend — C# / ASP.NET Core .NET 10

**Responsable:** Kort · **Base de datos MySQL V3:** Gael · **Frontend Angular/MVVM:** Fernando.

La API incluye los endpoints de salud existentes, autenticación de gerente demostrable sin base de datos (**US01**) y las operaciones de agenda de dos historias ya existentes:

- **US06 (#21):** crear citas del taller para CFMOTO ya registrada o motocicleta de otra marca (`EXTERNA`), sin crear cuentas/motos ajenas en la base.
- **US15 (#30):** consultar agenda diaria, consultar detalle, reprogramar y cancelar citas, guardando auditoría.

El cliente sigue solicitando por WhatsApp; **solo el personal autorizado** usa las operaciones de agenda.

## US01 — login de gerente de demostración (sin MySQL)

La US01 [incluye sus 12 CRC y enlaces al código](../docs/epicas/E01-identidad-acceso/US01.md). **El modo demo solo se activa en Development** y requiere definir una cuenta ficticia al ejecutar. No incluye contraseñas en Git, no crea una base de datos y no modifica el login MySQL que utiliza el resto del sistema.

Desde **PowerShell** en la raíz del repositorio:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:Auth__Demo__Enabled = "true"
$env:Auth__Demo__Correo = "gerente@example.invalid"
$env:Auth__Demo__Contrasena = "SoloPrueba123!"
$env:Auth__Demo__Nombre = "Gerente de demostración"
dotnet run --project backend/FollowMotors.Api/FollowMotors.Api.csproj
```

Enviar `POST http://localhost:5099/api/v1/auth/staff-login` con JSON `{"correo":"gerente@example.invalid","contrasena":"SoloPrueba123!"}`. El servidor genera una cookie de sesión; `GET /api/v1/auth/me` requiere enviar esa cookie y devuelve nombre/permisos. Una contraseña incorrecta, otro correo o una cuenta inactiva (`Auth__Demo__Activo=false`) devuelve 401. `POST /api/v1/auth/logout` cierra la sesión. No se implementa un login separado ni se conecta todavía la vista Angular.

Para volver al login MySQL, quitar `Auth__Demo__Enabled` (o establecer `false`) y configurar la conexión de desarrollo. **Nunca usar este usuario ficticio en un despliegue real**: en Production el código siempre utiliza `MySqlStaffAuthService`.

Pruebas automáticas sin MySQL: `dotnet run --project backend/FollowMotors.Api.Specs/FollowMotors.Api.Specs.csproj`.

## Ejecutar localmente

```bash
dotnet restore backend/FollowMotors.Api/FollowMotors.Api.csproj
dotnet run --project backend/FollowMotors.Api/FollowMotors.Api.csproj
```

API de desarrollo: http://localhost:5099. Para API y MySQL del Compose existente, configurar `.env` con contraseñas locales y ejecutar `docker compose up -d --build`.

**Importante:** la conexión del proyecto base apunta a `follow_motors`, que no contiene necesariamente las tablas de la versión nueva. Para probar US06 y US15, **primero cargar los SQL V3 de Gael en una base de desarrollo vacía** siguiendo `database/v3/sql/`, en orden 00–05; después establecer `ConnectionStrings__Default` con `Database=follow_motors_v3_dev`. No modificar la base anterior ni producción. Las consultas de salud no prueban por sí solas el esquema V3.

## Rutas de agenda (US06 + US15)

| Método | Ruta | Permiso del esquema V3 | Función |
| --- | --- | --- | --- |
| POST | `/api/v1/auth/staff-login` | Cuenta existente con permisos de agenda | Obtiene cookie de sesión |
| GET | `/api/v1/auth/me` | Cuenta autenticada | Identidad y permisos |
| POST | `/api/v1/auth/logout` | Cuenta autenticada | Cierra sesión |
| GET | `/api/v1/citas?fecha=2026-11-06` | `agenda.ver` | Agenda de un día |
| GET | `/api/v1/citas/{id}` | `agenda.ver` | Detalle |
| POST | `/api/v1/citas` | `agenda.crear` | Crear cita |
| PUT | `/api/v1/citas/{id}/reprogramar` | `agenda.modificar` | Cambiar horario |
| PUT | `/api/v1/citas/{id}/cancelar` | `agenda.modificar` | Cancelar con motivo |

**Sin usuario y rol de personal válidos, los endpoints protegidos devuelven HTTP 401/403.** El inicio de sesión valida el hash compatible con `PasswordHasher<T>` de ASP.NET Core Identity. No se cargan contraseñas por defecto, ni se incluyen datos reales de clientes.

### Ejemplo JSON — crear cita externa

```json
{
  "tipoMotocicleta": "EXTERNA",
  "motocicletaId": null,
  "marcaExterna": "Honda",
  "modeloExterno": "CB125",
  "contactoExterno": "Cliente de prueba",
  "telefonoExterno": "0000000000",
  "inicioLocal": "2026-11-06T11:00:00",
  "duracionMinutos": 60,
  "motivo": "Diagnóstico"
}
```

Para CFMOTO usar `tipoMotocicleta: "CFMOTO"`, `motocicletaId` de una motocicleta registrada y los campos externos en `null`. La fecha se envía como **hora local de México, sin Z ni offset**; no agregar reglas de agenda distintas de las acordadas.

### Reprogramar

```json
{ "inicioLocal": "2026-11-06T13:00:00", "duracionMinutos": 90 }
```

### Cancelar

```json
{ "motivo": "El cliente solicitó cancelar por WhatsApp." }
```

## SOLID, CRC y pruebas

- **S:** `CitasController` coordina HTTP; `AgendaService` casos de uso; `AgendaPolicy` reglas puras; `MySqlAgendaRepository` SQL/transacciones.
- **O:** se pueden ampliar validaciones/políticas sin modificar consultas o controladores.
- **L/D:** `IAgendaRepository` se sustituye por el fake de pruebas sin cambiar `AgendaService`.
- **I:** `IAgendaService` define solamente operaciones de agenda.
- **CRC:** [US06](../docs/epicas/E03-agenda-operacion/US06.md) y [US15](../docs/epicas/E03-agenda-operacion/US15.md), 12 fichas cada una.

```bash
dotnet run --project backend/FollowMotors.Api.Specs/FollowMotors.Api.Specs.csproj
```

Las pruebas comprueban casos correctos e incorrectos con un repositorio simulado; en **CI** se ejecuta además un segundo recorrido contra MySQL 8.4 real y los scripts V3 en una base efímera. Se prueban login, permisos, cita externa/CFMOTO, reprogramación, cancelación y tres eventos de auditoría. **No sustituye la prueba con la UI Angular y credenciales de un entorno de desarrollo gestionado por el equipo.**

## Seguridad y límites

- Ninguna acción de agenda acepta `registradaPor` del navegador. El actor procede de la cookie validada.
- El login solo admite cuentas existentes que tengan permisos desde `roles_permisos`; hay que aprovisionar cuentas y hashes por un proceso administrativo posterior.
- Cookie HttpOnly, SameSite Strict y Secure en producción; usar mismo origen o proxy Angular `/api`.
- Se consulta/vincula la base V3, no se modifica la base ni el código SQL del compañero.
- **Pendiente de integración:** conectar los formularios visuales de Fernando a estas rutas mediante Angular/HttpClient, sin reemplazar el diseño.
