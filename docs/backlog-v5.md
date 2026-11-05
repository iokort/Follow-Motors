# Product Backlog de referencia — 22 historias de usuario

**Línea base:** Planificación integral Follow Motors V5 (30/10/2026). Prioridad Alta/Media/Baja; puntos Fibonacci relativos (no horas); el valor y las dependencias se validan en refinamiento.

| ID | Historia | Épica | Prioridad | Puntos | Dependencias |
| --- | --- | --- | --- | ---: | --- |
| US01 | Acceso del gerente | Identidad y seguridad | Alta | 5 | US21 |
| US02 | Acceso del cliente | Identidad y seguridad | Alta | 5 | US21 |
| US03 | Cierre de sesión seguro | Identidad y seguridad | Alta | 3 | US01, US02 |
| US04 | Alta de cliente y motocicleta | Operación del taller | Alta | 8 | US21 |
| US05 | Administrar clientes y unidades | Operación del taller | Alta | 8 | US04 |
| US06 | Agenda de citas | Operación del taller | Alta | 8 | US04 |
| US07 | Seguimiento de mantenimiento | Operación del taller | Alta | 8 | US04 |
| US08 | Panel privado del cliente | Portal del cliente | Alta | 8 | US02, US07 |
| US09 | Portada de acceso público | Web pública | Alta | 3 | Ninguna |
| US10 | Catálogo público de servicios | Web pública | Alta | 5 | US09 |
| US11 | Información y preguntas frecuentes | Web pública | Media | 3 | US09 |
| US12 | Solicitud pública de contacto | Web pública | Media | 8 | US09 |
| US13 | Actualizar kilometraje propio | Portal del cliente | Alta | 5 | US02, US04 |
| US14 | Solicitar servicio adicional | Portal del cliente | Media | 5 | US08 |
| US15 | Agenda para asesor de servicio | Operación del taller | Media | 8 | US06, US21 |
| US16 | Órdenes de trabajo del mecánico | Operación del taller | Media | 8 | US07, US21 |
| US17 | Resumen operativo de gerencia | Operación del taller | Media | 5 | US06, US07 |
| US18 | Consulta para almacén | Expansiones condicionadas | Baja | 5 | US06, US21 |
| US19 | Registro de cobros | Expansiones condicionadas | Baja | 13 | US07, US21 |
| US20 | Prospectos y posventa | Expansiones condicionadas | Baja | 8 | US21 |
| US21 | Políticas de roles y permisos | Identidad y seguridad | Alta | 8 | Base técnica |
| US22 | Recordatorios de servicio | Expansiones condicionadas | Baja | 8 | US07, US08 |

**Total preliminar: 145 puntos**. No es una promesa de duración ni carga asignada. Las historias US18–US20 y US22 se consideran alcance diferido hasta aprobación.

## Tarjetas de historia (resumen con criterio de aceptación)

### US01 · Acceso del gerente

**Como** Gerente, **quiero** iniciar sesión y acceder al área administrativa con autorización del servidor.

- **Prioridad / estimación:** Alta, 5 puntos.
- **Dependencia:** US21.
- **Criterios verificables:** Credenciales válidas abren sesión gerencial; errores no revelan cuentas; fallo de red no abre área privada.
- **Evidencia futura:** Issue `US01`, PR vinculado, resultado de prueba `CP-01` y demostración cuando exista.


### US02 · Acceso del cliente

**Como** Cliente, **quiero** iniciar sesión voluntariamente y consultar únicamente los datos propios.

- **Prioridad / estimación:** Alta, 5 puntos.
- **Dependencia:** US21.
- **Criterios verificables:** La sesión solo consulta recursos propios; datos vacíos o incorrectos muestran errores accesibles; sesión vencida no expone datos.
- **Evidencia futura:** Issue `US02`, PR vinculado, resultado de prueba `CP-02` y demostración cuando exista.


### US03 · Cierre de sesión seguro

**Como** Persona autenticada, **quiero** cerrar la sesión e impedir la recuperación de datos privados.

- **Prioridad / estimación:** Alta, 3 puntos.
- **Dependencia:** US01, US02.
- **Criterios verificables:** Invalidación de sesión en servidor; navegación atrás no expone datos; sitio público continúa accesible.
- **Evidencia futura:** Issue `US03`, PR vinculado, resultado de prueba `CP-03` y demostración cuando exista.


### US04 · Alta de cliente y motocicleta

**Como** Gerente, **quiero** registrar y vincular clientes y motocicletas sin duplicación.

- **Prioridad / estimación:** Alta, 8 puntos.
- **Dependencia:** US21.
- **Criterios verificables:** Alta válida crea registros relacionados; datos inválidos no se guardan; cliente existente añade motocicleta sin duplicarse.
- **Evidencia futura:** Issue `US04`, PR vinculado, resultado de prueba `CP-04` y demostración cuando exista.


### US05 · Administrar clientes y unidades

**Como** Gerente, **quiero** buscar y actualizar expedientes de clientes y motocicletas.

- **Prioridad / estimación:** Alta, 8 puntos.
- **Dependencia:** US04.
- **Criterios verificables:** Búsqueda autorizada por datos clave; VIN y modelo consultables; modificaciones preservan relaciones e historial.
- **Evidencia futura:** Issue `US05`, PR vinculado, resultado de prueba `CP-05` y demostración cuando exista.


### US06 · Agenda de citas

**Como** Gerente o asesor, **quiero** crear y reprogramar citas vinculadas a una motocicleta.

- **Prioridad / estimación:** Alta, 8 puntos.
- **Dependencia:** US04.
- **Criterios verificables:** Cita válida aparece en agenda; VIN inexistente se valida; disponibilidad evita reservas incompatibles.
- **Evidencia futura:** Issue `US06`, PR vinculado, resultado de prueba `CP-06` y demostración cuando exista.


### US07 · Seguimiento de mantenimiento

**Como** Gerente o mecánico, **quiero** registrar trabajos y avances con historial verificable.

- **Prioridad / estimación:** Alta, 8 puntos.
- **Dependencia:** US04.
- **Criterios verificables:** Etapas visibles; cada servicio registra fecha, km y responsable; historial no sobrescribe eventos anteriores.
- **Evidencia futura:** Issue `US07`, PR vinculado, resultado de prueba `CP-07` y demostración cuando exista.


### US08 · Panel privado del cliente

**Como** Cliente, **quiero** consultar motos propias, historial y próximo mantenimiento.

- **Prioridad / estimación:** Alta, 8 puntos.
- **Dependencia:** US02, US07.
- **Criterios verificables:** Solo unidades propias; vacío explicado; próxima recomendación depende de regla documentada.
- **Evidencia futura:** Issue `US08`, PR vinculado, resultado de prueba `CP-08` y demostración cuando exista.


### US09 · Portada de acceso público

**Como** Visitante, **quiero** conocer el taller sin necesidad de crear una cuenta.

- **Prioridad / estimación:** Alta, 3 puntos.
- **Dependencia:** Ninguna.
- **Criterios verificables:** Home pública sin redirección forzosa; servicios visibles; login opcional.
- **Evidencia futura:** Issue `US09`, PR vinculado, resultado de prueba `CP-09` y demostración cuando exista.


### US10 · Catálogo público de servicios

**Como** Visitante, **quiero** explorar tipos y descripciones de servicios.

- **Prioridad / estimación:** Alta, 5 puntos.
- **Dependencia:** US09.
- **Criterios verificables:** Accesible sin cookie; sin datos de expedientes; navegación a contacto o regreso.
- **Evidencia futura:** Issue `US10`, PR vinculado, resultado de prueba `CP-10` y demostración cuando exista.


### US11 · Información y preguntas frecuentes

**Como** Visitante, **quiero** consultar información y proceso de atención.

- **Prioridad / estimación:** Media, 3 puntos.
- **Dependencia:** US09.
- **Criterios verificables:** FAQ sin sesión; datos de contacto solamente verificados; navegación coherente.
- **Evidencia futura:** Issue `US11`, PR vinculado, resultado de prueba `CP-11` y demostración cuando exista.


### US12 · Solicitud pública de contacto

**Como** Visitante, **quiero** enviar una consulta sin crear cuenta.

- **Prioridad / estimación:** Media, 8 puntos.
- **Dependencia:** US09.
- **Criterios verificables:** Datos mínimos y privacidad; solicitud pendiente y no cita confirmada; protección ante abuso.
- **Evidencia futura:** Issue `US12`, PR vinculado, resultado de prueba `CP-12` y demostración cuando exista.


### US13 · Actualizar kilometraje propio

**Como** Cliente, **quiero** registrar el odómetro de una motocicleta propia.

- **Prioridad / estimación:** Alta, 5 puntos.
- **Dependencia:** US02, US04.
- **Criterios verificables:** Solo unidades propias; corrección hacia atrás necesita revisión; historial de lectura/actor/fecha.
- **Evidencia futura:** Issue `US13`, PR vinculado, resultado de prueba `CP-13` y demostración cuando exista.


### US14 · Solicitar servicio adicional

**Como** Cliente, **quiero** pedir atención para mantenimiento no programado.

- **Prioridad / estimación:** Media, 5 puntos.
- **Dependencia:** US08.
- **Criterios verificables:** Solicitud vinculada a cuenta y unidad; personal autorizado la gestiona; estados visibles sin reserva automática.
- **Evidencia futura:** Issue `US14`, PR vinculado, resultado de prueba `CP-14` y demostración cuando exista.


### US15 · Agenda para asesor de servicio

**Como** Asesor, **quiero** consultar disponibilidad y gestionar citas asignadas.

- **Prioridad / estimación:** Media, 8 puntos.
- **Dependencia:** US06, US21.
- **Criterios verificables:** Alcance autorizado; cambios auditables; sin privilegios gerenciales.
- **Evidencia futura:** Issue `US15`, PR vinculado, resultado de prueba `CP-15` y demostración cuando exista.


### US16 · Órdenes de trabajo del mecánico

**Como** Mecánico, **quiero** consultar y actualizar trabajos asignados.

- **Prioridad / estimación:** Media, 8 puntos.
- **Dependencia:** US07, US21.
- **Criterios verificables:** Solo órdenes permitidas; avances controlados; no altera identidad ni precios administrativos.
- **Evidencia futura:** Issue `US16`, PR vinculado, resultado de prueba `CP-16` y demostración cuando exista.


### US17 · Resumen operativo de gerencia

**Como** Gerente, **quiero** identificar citas, solicitudes y servicios pendientes.

- **Prioridad / estimación:** Media, 5 puntos.
- **Dependencia:** US06, US07.
- **Criterios verificables:** Conteos de datos autorizados; tarjetas enlazan listas filtradas; cambios actualizan métricas.
- **Evidencia futura:** Issue `US17`, PR vinculado, resultado de prueba `CP-17` y demostración cuando exista.


### US18 · Consulta para almacén

**Como** Almacén, **quiero** consultar necesidades de refacciones de servicios próximos.

- **Prioridad / estimación:** Baja, 5 puntos.
- **Dependencia:** US06, US21.
- **Criterios verificables:** Sin acceso a clientes o cobros; inventario de requerimientos sin doble captura.
- **Evidencia futura:** Issue `US18`, PR vinculado, resultado de prueba `CP-18` y demostración cuando exista.


### US19 · Registro de cobros

**Como** Caja, **quiero** registrar cobros vinculados a trabajos autorizados.

- **Prioridad / estimación:** Baja, 13 puntos.
- **Dependencia:** US07, US21.
- **Criterios verificables:** Servicio y usuario trazables; rectificaciones auditadas; no sustituye facturación fiscal.
- **Evidencia futura:** Issue `US19`, PR vinculado, resultado de prueba `CP-19` y demostración cuando exista.


### US20 · Prospectos y posventa

**Como** Vendedor, **quiero** registrar prospectos y seguimiento comercial.

- **Prioridad / estimación:** Baja, 8 puntos.
- **Dependencia:** US21.
- **Criterios verificables:** Separación de clientes activos; minimización de datos; sin acceso a expedientes mecánicos.
- **Evidencia futura:** Issue `US20`, PR vinculado, resultado de prueba `CP-20` y demostración cuando exista.


### US21 · Políticas de roles y permisos

**Como** Gerente autorizado/TI, **quiero** controlar acceso por rol y pertenencia del recurso desde API.

- **Prioridad / estimación:** Alta, 8 puntos.
- **Dependencia:** Base técnica.
- **Criterios verificables:** API valida sesión, rol y propiedad; rol desconocido denegado; cambios auditados y probados.
- **Evidencia futura:** Issue `US21`, PR vinculado, resultado de prueba `CP-21` y demostración cuando exista.


### US22 · Recordatorios de servicio

**Como** Cliente, **quiero** elegir si recibe avisos voluntarios de próximos servicios.

- **Prioridad / estimación:** Baja, 8 puntos.
- **Dependencia:** US07, US08.
- **Criterios verificables:** Consentimiento revocable; fallos de notificación no alteran datos de servicio.
- **Evidencia futura:** Issue `US22`, PR vinculado, resultado de prueba `CP-22` y demostración cuando exista.



## Orden sugerido por dependencias

1. **Web pública:** US09 → US10 (sin cuenta).
2. **Seguridad:** US21 → US01/US02 → US03.
3. **Datos:** US04 → US05.
4. **Operación:** US06/US07 → US08.
5. **Autoservicio y operadores:** US13–US17.
6. **Ampliaciones sujetas a decisión:** US18–US20, US22.

La historia sin criterio verificable o contrato acordado permanece en Backlog/Preparado; no debe indicarse como completada.
