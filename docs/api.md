# Contratos API de Follow Motors

**Versión:** `/api/v1` · **Tecnología:** ASP.NET Core .NET 10 · **Base:** MySQL V3 de Gael.

## Endpoints técnicos existentes

| Método | URL | Resultado |
| --- | --- | --- |
| GET | `/api/v1/health` | `200`, salud de API |
| GET | `/api/v1/health/database` | `200/503`, conectividad MySQL; solo Development |

## US06 — Crear cita

`POST /api/v1/citas` requiere cookie del personal autenticado y permiso `agenda.crear`.

Ejemplo de moto de otra marca:

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

Ejemplo CFMOTO: `tipoMotocicleta: "CFMOTO"`, un `motocicletaId` GUID de una unidad **activa en el portal** y campos externos `null`.

Devuelve `201 Created`, la cita creada y `Location` del recurso. `400` para datos inválidos, `401` sin sesión, `403` sin permiso. Si MySQL no está configurado o falta el esquema V3, devuelve error de servidor (no hay una cita simulada).

**No hay autorregistro ni agenda directa de clientes**; la solicitud llega por WhatsApp y el gerente la introduce.

## US15 — Consulta, reprogramación y cancelación

| Método | URL | Permiso | Respuesta |
| --- | --- | --- | --- |
| GET | `/api/v1/citas?fecha=AAAA-MM-DD` | `agenda.ver` | `200` con hasta 200 citas del día |
| GET | `/api/v1/citas/{id}` | `agenda.ver` | `200` cita o `404` |
| PUT | `/api/v1/citas/{id}/reprogramar` | `agenda.modificar` | `200` cita actualizada, `404`, `409` o `400` |
| PUT | `/api/v1/citas/{id}/cancelar` | `agenda.modificar` | `200` con estado CANCELADA, `404`, `409` o `400` |

Reprogramación (`application/json`):

```json
{ "inicioLocal": "2026-11-07T12:30:00", "duracionMinutos": 90 }
```

Cancelación (`application/json`):

```json
{ "motivo": "La persona canceló por WhatsApp" }
```

**Transiciones:** se pueden modificar citas `PROGRAMADA` o `CONFIRMADA`. Las `CANCELADA` o `REALIZADA` no se alteran (`409 Conflict`). La fecha nueva debe ser futura. Las modificaciones no borran registros: quedan en `auditoria` con identificador del actor autenticado.

## Dependencia de autenticación del personal

`POST /api/v1/auth/staff-login` recibe JSON con `correo` y `contrasena`. Valida la cuenta existente, hash compatible con ASP.NET Core Identity y permisos de `roles_permisos`; devuelve una cookie HttpOnly de sesión si la cuenta está autorizada. No hay endpoint público para crear usuarios.

- `GET /api/v1/auth/me`: sesión actual y permisos.
- `POST /api/v1/auth/logout`: cerrar sesión.
- Sin cookie: `401`. Sin permiso requerido: `403`. Contraseña errónea: `401`.

**Seguridad:** SameSite Strict, cookie HttpOnly, Secure en producción; no enviar actor de auditoría desde Angular; para integrar la UI usar mismo origen o proxy `/api`.

## Estado de implementación

Estos endpoints fueron incorporados como backend en el [PR #48](https://github.com/iokort/Follow-Motors/pull/48), con comentarios XML, SOLID, pruebas unitarias y verificación MySQL V3 en GitHub Actions. Su documentación no implica que la interfaz Angular ya envíe peticiones reales ni que existan cuentas de gerente aprovisionadas fuera de pruebas.

Los contratos de clientes, servicios de mantenimiento, garantías y nuevas cuentas siguen pendientes de implementación: no aparecen como endpoints terminados.

[CRC US06](epicas/E03-agenda-operacion/US06.md) · [CRC US15](epicas/E03-agenda-operacion/US15.md) · [Guía backend](../backend/README.md).
