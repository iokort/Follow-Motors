# Backend — C# / ASP.NET Core .NET 10

**Responsable:** Kort · **Base de datos MySQL V3:** Gael · **Frontend Angular/MVVM:** Fernando.

La API incluye autenticación gerencial real (US01), endpoints de salud y dos historias de agenda en C# bajo el plan Scrum del equipo:

- **US06 (#21):** crear citas del taller para CFMOTO ya registrada o motocicleta de otra marca (`EXTERNA`), sin crear cuentas/motos ajenas en la base.
- **US15 (#30):** consultar agenda diaria, consultar detalle, reprogramar y cancelar citas, guardando auditoría.

El cliente sigue solicitando por WhatsApp; **solo el personal autorizado** usa las operaciones de agenda.

## US01 — Autenticación del gerente con MySQL V3

[Historia US01 con 12 tarjetas CRC y enlaces al código](../docs/epicas/E01-identidad-acceso/US01.md).

El inicio de sesión utiliza las tablas reales `usuarios`, `usuarios_roles`, `roles` y `roles_permisos` de Gael. No hay usuarios ni contraseñas de demostración en la API: la cuenta debe existir, estar activa y tener un hash `PasswordHasher<T>` válido. `GERENTE` y `ASESOR` no son lo mismo; el acceso gerencial requiere expresamente el rol `GERENTE`.

1. Ejecutar los SQL V3 (00–05) en la base de desarrollo aislada. **No aplicar sobre producción sin coordinación**.
2. Configurar `ConnectionStrings__Default` con el servidor, usuario y contraseña MySQL de tu entorno local (no subir secretos a Git).
3. Iniciar la API con el comando de la sección siguiente.
4. Consumir `POST /api/v1/auth/staff-login` con JSON `{"correo":"correo_registrado","contrasena":"contraseña_registrada"}`; el servidor devuelve la cookie firmada.
5. Con esa cookie, consultar `GET /api/v1/auth/me` o `GET /api/v1/auth/gerencia/acceso`. Esta última devuelve **200** para GERENTE, **403** para otros roles y **401** sin sesión. Cerrar con `POST /api/v1/auth/logout`.

La cookie es `HttpOnly`, `SameSite=Strict`, `Secure` en producción y caduca a las 6 horas. Angular consumirá la API desde el mismo origen; la UI todavía no está conectada. El backend no crea cuentas: el alta administrativa es otro caso de uso.

**Pruebas de integración:** [`IntegrationSpecs.cs`](FollowMotors.Api.Specs/IntegrationSpecs.cs) valida la autenticación con MySQL V3 real en CI, incluyendo roles, permisos, cuentas activas/inactivas y contraseñas incorrectas.

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
