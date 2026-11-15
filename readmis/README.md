# Documentación de FollowMotors

Este directorio es la fuente de verdad del **nuevo análisis funcional**. Se redactó desde el modelo real del taller; las decisiones heredadas de ramas o Issues anteriores no se adoptan automáticamente.

## Orden de lectura

| Documento | Contenido |
| --- | --- |
| [00 — Concepto](00-concepto.md) | Problema, actores, productos, límites y operaciones |
| [01 — Reglas de negocio](01-reglas-negocio.md) | Decisiones vigentes con identificadores NEG |
| [02 — Flujos](02-flujos.md) | Alta, cita, diagnóstico, autorización, asignación, refacciones, avance, cancelación y entrega |
| [03 — Arquitectura y GitHub](03-arquitectura-github.md) | Capas, SOLID, CQRS, validaciones, pruebas y GitHub Flow |
| [04 — Trazabilidad y pruebas](04-trazabilidad-pruebas.md) | Correspondencia de épicas, historias y cobertura esperada |

## Épicas

1. [E01 — Identidad y expedientes](epicas/E01-identidad-expedientes/README.md): [US01](epicas/E01-identidad-expedientes/US01.md) · [US02](epicas/E01-identidad-expedientes/US02.md) · [US03](epicas/E01-identidad-expedientes/US03.md)
2. [E02 — Citas y recepción](epicas/E02-citas-recepcion/README.md): [US04](epicas/E02-citas-recepcion/US04.md) · [US05](epicas/E02-citas-recepcion/US05.md) · [US06](epicas/E02-citas-recepcion/US06.md)
3. [E03 — Contratación y recursos](epicas/E03-contratacion-recursos/README.md): [US07](epicas/E03-contratacion-recursos/US07.md) · [US08](epicas/E03-contratacion-recursos/US08.md) · [US09](epicas/E03-contratacion-recursos/US09.md)
4. [E04 — Ejecución y cierre](epicas/E04-ejecucion-cierre/README.md): [US10](epicas/E04-ejecucion-cierre/US10.md) · [US11](epicas/E04-ejecucion-cierre/US11.md) · [US12](epicas/E04-ejecucion-cierre/US12.md)

## Criterios de documentación

- **Confirmado:** acuerdos expresos del negocio. **Diseño operativo:** decisiones razonables para hacer implementable el sistema, revisables antes de programar.
- Las tarjetas CRC describen clase candidata, responsabilidad y colaboradores; no imponen una clase por historia ni una interfaz por operación.
- La trazabilidad usa códigos `NEG-xx`, `E0x` y `USxx`.
- El avance de reparación se basa en tareas del trabajo, **no en días transcurridos**.
- Una solicitud de cancelación no equivale a una cancelación autorizada; el servidor debe resolverla de manera consistente.
