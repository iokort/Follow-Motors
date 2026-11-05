# Contratos API

Versión inicial de rutas: `/api/v1`; cuerpo y respuestas en JSON.

## Endpoints existentes
| Método | Endpoint | Respuesta | Acceso |
| --- | --- | --- | --- |
| GET | `/api/v1/health` | `200 {"status":"ok","service":"follow-motors-api"}` | Cualquier entorno |
| GET | `/api/v1/health/database` | `200 {"status":"ok","database":"mysql"}` o problema HTTP 503 | Solo Development |

**Estas rutas solo verifican salud técnica. No ofrecen operaciones de negocio.**

## Por diseñar con el equipo
| Área | Contrato | Responsable |
| --- | --- | --- |
| Login/registro | Pendiente | Kort, Vixtor Fernando |
| Clientes y cuentas | Pendiente | Kort, Gael, Vixtor Fernando |
| Unidades | Pendiente | Kort, Gael, Vixtor Fernando |
| Kilometraje | Pendiente | Kort, Gael, Vixtor Fernando |
| Agenda y servicios | Pendiente | Kort, Gael, Vixtor Fernando |
| Garantías | Pendiente | Kort, Gael, Vixtor Fernando |

Para cada ruta se documentará método, URL, actor/rol, parámetros, DTO JSON, respuesta exitosa, validaciones, errores HTTP y pruebas. No introducir rutas especulativas como si estuvieran aprobadas.
