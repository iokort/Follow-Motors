# Backlog Scrum — Cuatro épicas y doce historias

Este es el **plan funcional priorizado** de SSC. Conserva los identificadores de las historias de la planificación V5 y añade US23 para garantías, basada en el [Issue #38](https://github.com/iokort/Follow-Motors/issues/38).

**Cómputo:** 4 épicas × 3 historias por épica = **12 US**; cada US incluye **12 tarjetas CRC**, para **144 CRC de análisis**. Una ficha CRC describe clase/componente candidato, responsabilidad y colaboradores; no equivale a código ya implementado ni obliga a crear 144 clases independientes.

| Épica | Resultado | Historias | CRC |
| --- | --- | --- | ---: |
| [E01 · Identidad y permisos](E01-identidad-acceso/README.md) · [Épica #43](https://github.com/iokort/Follow-Motors/issues/43) | Acceso seguro y autorizaciones | [US01](E01-identidad-acceso/US01.md) · [US02](E01-identidad-acceso/US02.md) · [US21](E01-identidad-acceso/US21.md) | 36 |
| [E02 · Expedientes y motocicletas](E02-expedientes-unidades/README.md) · [Épica #44](https://github.com/iokort/Follow-Motors/issues/44) | Alta presencial, gestión y odómetro | [US04](E02-expedientes-unidades/US04.md) · [US05](E02-expedientes-unidades/US05.md) · [US13](E02-expedientes-unidades/US13.md) | 36 |
| [E03 · Agenda y operación](E03-agenda-operacion/README.md) · [Épica #45](https://github.com/iokort/Follow-Motors/issues/45) | Citas internas, otras marcas y taller | [US06](E03-agenda-operacion/US06.md) · [US15](E03-agenda-operacion/US15.md) · [US16](E03-agenda-operacion/US16.md) | 36 |
| [E04 · Mantenimiento, garantías y cliente](E04-mantenimiento-garantias-portal/README.md) · [Épica #46](https://github.com/iokort/Follow-Motors/issues/46) | Etapas, decisiones y consulta privada | [US07](E04-mantenimiento-garantias-portal/US07.md) · [US08](E04-mantenimiento-garantias-portal/US08.md) · [US23](E04-mantenimiento-garantias-portal/US23.md) | 36 |
| **Total** | | **12 historias** | **144** |

## Prioridades reales de la entrega escolar

**Los tres módulos que el equipo quiere mostrar son:** gestión de garantías y seguimiento, agenda de citas y panel del cliente con sus motocicletas. Sus dependencias mínimas son identidad autorizada, expedientes y permisos. No se exige construir todo el sistema empresarial para la primera exposición.

El resto de las historias del [backlog V5 original](../backlog-v5.md), incluidas la portada informativa, FAQ, cobros, almacén y notificaciones, permanece como referencia o alcance posterior. No se han borrado sus Issues.

## Reglas funcionales vigentes

1. El gerente ejerce también de asesor por ahora, registra clientes/unidades en sucursal y entrega credenciales.
2. El cliente no crea cuentas, no vincula motos y no agenda citas desde la web. Solicita atención por WhatsApp; el personal la programa.
3. La agenda recibe citas de **otras marcas**. Según el esquema V3 de Gael, estas citas son `EXTERNA` y **no se convierten automáticamente** en motos CFMOTO del portal privado.
4. Se registran tres servicios de garantía; atrasarse no impide registrar. El gerente decide y fundamenta si afecta garantía.
5. Al completar los tres servicios, la garantía finaliza; pueden existir mantenimientos posteriores cada seis meses.
6. El gerente puede corregir errores, conservando actor, fecha y motivo en la auditoría.
7. Los clientes solo consultan sus propias unidades, servicios y estado de garantía.
8. **Discrepancia a resolver antes del cálculo definitivo:** SQL V3 codifica 3, 6 y 12 meses **desde la venta**, mientras la formulación inicial puede interpretarse como intervalos **entre servicios**. La implementación no debe cambiar los scripts de Gael sin acordarlo expresamente.

## Definición de «terminado» para cada historia

- Criterios de aceptación probados con datos sintéticos.
- Responsable y PR localizables, con CI satisfactoria y revisión.
- API .NET valida permisos y titularidad; frontend Angular presenta estados de carga/error.
- Si implica MySQL, scripts/migración reproducibles y revisión de Gael.
- Aarón conserva trazabilidad entre la historia, prueba y evidencia real.
- El estado `Done` de GitHub Projects solo se usa tras verificar lo anterior.

[Plan Scrum de trabajo](../scrum-plan-ssc.md) · [Stakeholders y RACI](../stakeholders-ssc.md) · [Arquitectura técnica](../arquitectura-objetivo-ssc.md).
