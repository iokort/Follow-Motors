# Reglas de negocio vigentes — Follow Motors

**Origen:** respuestas funcionales del equipo (Gael, noviembre de 2026), frontend de Fernando y SQL V3 de Gael. Este documento orienta el desarrollo; si el código existente difiere, se abre una Issue de ajuste en lugar de eliminar el trabajo del compañero.

| ID | Regla confirmada | Consecuencia técnica |
| --- | --- | --- |
| RN01 | Empresa de seguimiento de motocicletas y **posventa**. El portal no vende en línea. | Página pública informativa y portal privado |
| RN02 | El gerente desempeña también la función de asesor por ahora, registra clientes y entrega credenciales. | Control de permisos desde .NET y cuenta creada en sucursal |
| RN03 | El cliente no se autorregistra ni añade motos desde internet. | No exponer endpoints de autorregistro; validaciones backend |
| RN04 | El cliente puede consultar exclusivamente sus motos y datos autorizados. | Verificación de titularidad en cada operación de API |
| RN05 | El personal programa citas; el cliente las solicita por WhatsApp. | Agenda interna, sin formulario público que guarde citas directamente |
| RN06 | Pueden agendarse motos de otras marcas. | Esquema V3 usa `citas.tipo_motocicleta='EXTERNA'` sin `motocicleta_id`; moto del portal sigue siendo CFMOTO |
| RN07 | Existen tres servicios de mantenimiento/garantía. Se registran fecha y kilometraje. | `etapas_mantenimiento`, `trabajos`, `planes_mantenimiento` |
| RN08 | Un servicio fuera de plazo se puede registrar; el gerente decide si invalida garantía. | `decisiones_garantia` con autor y motivo |
| RN09 | Al concluir tres servicios finaliza garantía, pero continúan mantenimientos cada 6 meses. | Separar `FINALIZADA` de trabajos `POST_GARANTIA` |
| RN10 | El gerente puede corregir errores. | Respetar auditoría de corrección; no eliminar historial |
| RN11 | Primera exposición: garantías, agenda y panel privado del cliente. | Ordenar Scrum por dependencias de estos tres módulos |
| RN12 | Stack: Angular 22 + TypeScript + MVVM, C#/.NET 10 + SOLID, MySQL 8.4. | Servicios HTTP separados, MySQL solo servidor |

## Decisión de fechas que aún debe conciliarse con Gael

La respuesta «la primera regla» se refería a **3 meses, luego 6, luego 12**, pero la redacción inicial permite dos lecturas:

- **Meses desde venta:** 3, 6 y 12 meses. Es lo que implementa hoy `database/v3/sql/05_catalogos_iniciales.sql` y el campo `etapas_mantenimiento.meses_desde_venta` de Gael.
- **Intervalos entre servicios:** primero a 3 meses desde la venta, segundo 6 meses después del primero y tercero 12 después del segundo. No es equivalente.

**No cambiar unilateralmente los scripts V3.** Antes de implementar el cálculo de fechas en C#, Gael y Kort deben dejar registrada la interpretación final en una Issue/ADR. El cálculo posterior al tercer servicio (cada seis meses) es una regla independiente.

## Diferencia entre estado de demo e implementación

El frontend de Fernando presenta varias pantallas con datos de demostración; actualmente Angular las integra mediante un adaptador. Esto prueba que existe la interfaz, **no** autenticación real, persistencia de citas o decisiones de garantía en MySQL. Los scripts V3 están en el repo; no se afirma su ejecución en la base de un servidor real.

[Épicas y US](epicas/README.md) · [Matriz stakeholders](stakeholders-ssc.md) · [Arquitectura](arquitectura-objetivo-ssc.md).
