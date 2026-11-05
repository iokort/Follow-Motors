# E03 — Agenda y operación de taller

**Resultado de negocio:** El gerente agenda citas, incluso de otras marcas, y el taller puede registrar trabajos. El cliente contacta por WhatsApp, no reserva desde la web.

| Historia | Issue | Prioridad | CRC |
| --- | --- | --- | ---: |
| [US06: Programar citas para cualquier marca](US06.md) | [#21](https://github.com/iokort/Follow-Motors/issues/21) | Alta | 12 |
| [US15: Consultar, reprogramar y cancelar agenda](US15.md) | [#30](https://github.com/iokort/Follow-Motors/issues/30) | Alta | 12 |
| [US16: Registrar órdenes y trabajos de mecánico](US16.md) | [#31](https://github.com/iokort/Follow-Motors/issues/31) | Media | 12 |

**Trazabilidad:** responsabilidades candidatas, no clases ya implementadas. [Volver al índice](../README.md).

## Avance técnico del backend

El [PR #48](https://github.com/iokort/Follow-Motors/pull/48) implementa los endpoints C# de US06 y US15, con servicio, política, repositorio MySQL V3 y pruebas. La integración Angular, el aprovisionamiento de cuentas y la demo extremo a extremo siguen pendientes; las US **no están cerradas**.
