# 04 — Trazabilidad y criterios de prueba

## Matriz épica → historia → objetivo

| Épica | Historia | Resultado comprobable |
| --- | --- | --- |
| E01 | [US01](epicas/E01-identidad-expedientes/US01.md) | Alta presencial y control de cuentas |
| E01 | [US02](epicas/E01-identidad-expedientes/US02.md) | Moto multimarca vinculada sin duplicados |
| E01 | [US03](epicas/E01-identidad-expedientes/US03.md) | Portal privado y consultas autorizadas |
| E02 | [US04](epicas/E02-citas-recepcion/US04.md) | Reserva web con confirmación inmediata |
| E02 | [US05](epicas/E02-citas-recepcion/US05.md) | Administración de agenda y capacidad |
| E02 | [US06](epicas/E02-citas-recepcion/US06.md) | Recepción y diagnóstico de tarifa fija |
| E03 | [US07](epicas/E03-contratacion-recursos/US07.md) | Cotización y aceptación presencial |
| E03 | [US08](epicas/E03-contratacion-recursos/US08.md) | Anticipo y condiciones de garantía |
| E03 | [US09](epicas/E03-contratacion-recursos/US09.md) | Asignación y refacciones de proveedor |
| E04 | [US10](epicas/E04-ejecucion-cierre/US10.md) | Tareas ponderadas y seguimiento |
| E04 | [US11](epicas/E04-ejecucion-cierre/US11.md) | Cancelación y reembolso con umbral |
| E04 | [US12](epicas/E04-ejecucion-cierre/US12.md) | Entrega, historial y garantía consultable |

## Definición de hecho (DoD) por historia

- Criterios de aceptación implementados y demostrables con un escenario positivo y uno negativo.
- Reglas `NEG` asociadas verificadas.
- Unit tests para lógica con decisiones y un test de integración si se involucra persistencia/HTTP.
- Autorización por usuario y propiedad de datos comprobada cuando corresponda.
- PR revisado y sin secretos; cambio enfocado solo en esa historia.
- Documentación actualizada si el negocio cambió.

## Casos transversales críticos

| Caso | Resultado esperado |
| --- | --- |
| Dos clientes reservan el último cupo simultáneamente | Una sola reserva consume el cupo disponible |
| Cliente consulta un VIN/cita/orden de otro | API devuelve acceso denegado o recurso inexistente sin filtrar datos |
| Revisión rechazada | Diagnóstico pagado y guardado; no hay orden de reparación |
| Proveedor demora una pieza | Orden espera y mecánico puede atender otra |
| Piezas selladas no usadas en cancelación | Retorno al proveedor, importe devuelto al cliente |
| Progreso exactamente 60 % | Cancelación aún permitida |
| Progreso 60.01 % | Cancelación rechazada; taller finaliza trabajo |
| Cancelación simultánea al registro de tarea | Una única resolución coherente sin doble reembolso |
| Tareas suman 90 % o 110 % | Plan no puede activarse |
| Cuenta desactivada intenta iniciar sesión | Acceso rechazado, historial preservado |
| Vencimiento por calendario vs tareas | El progreso NO aumenta con días transcurridos |

**Nota de alcance:** quedan excluidos despliegue público, inventario integral, sistema de facturación fiscal y cobros electrónicos. Los importes de ejemplos y pruebas serán ficticios.
