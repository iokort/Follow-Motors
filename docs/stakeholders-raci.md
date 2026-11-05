# Stakeholders, gobierno y RACI

## Mapa de interesados (línea base V5)

| ID | Interesado | Prioridad | Interés |
| --- | --- | --- | --- |
| ST01 | Dirección / propietario | Alta | Validar alcance e inversión |
| ST02 | Gerencia / administración | Alta | Clientes, agenda, servicios |
| ST03 | Clientes propietarios | Alta | Privacidad y seguimiento |
| ST04 | Asesores de servicio | Media | Recepción y programación |
| ST05 | Desarrollo y QA | Alta | Calidad y pruebas |
| ST06 | TI / seguridad | Alta | Identidad, protección y respaldo |
| ST07 | Infraestructura | Media | Alojamiento y recuperación |
| ST08 | Mecánicos | Media | Órdenes e historial |
| ST09 | Vendedores | Baja | Posventa futura |
| ST10 | Caja | Baja | Cobros futuros |
| ST11 | Almacén | Baja | Refacciones futuras |
| ST12 | Asesor académico | Alta | Revisión del proceso y entregables |

Los actores de negocio no equivalen a integrantes del equipo de GitHub. La participación de gerencia y clientes debe confirmarse, no inventarse.

## RACI del equipo SSC

R = ejecuta, A = responsable final, C = consultado, I = informado. Se limita a organización del **trabajo académico**, no a decisiones de negocio externas.

| Actividad | Kort | Gael | Vixtor Fernando | Aarón |
| --- | --- | --- | --- | --- |
| Coordinación técnica y contratos | A/R | C | C | I |
| Modelado SQL y migraciones | C | A/R | I | C |
| Interfaz, navegación y accesibilidad | C | I | A/R | C |
| Requisitos y criterios | C | C | C | A/R |
| Seguridad de API y roles | A/R | C | C | I |
| Integración y CI | A/R | C | C | I |
| Actualización del expediente | C | C | C | A/R |
| Preparación de demo y evidencia | A | C | C | R |

**Aceptación funcional del negocio:** depende de un representante autorizado; no se deduce de que la tarea tenga assignee ni de un PR fusionado.

## Riesgos y bloqueos iniciales

| Riesgo | Mitigación |
| --- | --- |
| Cambios no coordinados de contratos y esquema | Acordar DTO/campos y usar PR con revisores |
| Roles/autorizaciones incompletos | Matriz de permisos, pruebas negativas |
| Datos sensibles en repo público | Datos sintéticos, protección de secretos |
| Falta de hosting para API .NET | Validación de entorno antes de liberar |
| Tablero engañoso por estados incorrectos | Marcar solo tareas realmente iniciadas |
