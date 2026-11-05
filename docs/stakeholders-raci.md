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

R = responsable de ejecutar; A = responsable final de ese resultado; C = consultado; I = informado. Cada fila tiene un A principal. Estas responsabilidades corresponden al **proyecto académico**, no a la dirección de la empresa.

| Actividad | Kort · Backend | Gael · Database | Vixtor Fernando · Frontend | Aarón · Project Manager |
| --- | --- | --- | --- | --- |
| Planificación, seguimiento, reuniones y bloqueos | C | C | C | **A/R** |
| Enfoque funcional y propuesta de prioridades del backlog | C | **A/R** | C | R |
| Modelo relacional, migraciones y consultas MySQL | C | **A/R** | I | I |
| Diseño visual, Angular, navegación y accesibilidad | C | I | **A/R** | I |
| API C#, reglas de servidor y autorización | **A/R** | C | C | I |
| Contratos HTTP e integración de componentes | **A/R** | C | R | I |
| Documentación y trazabilidad del expediente | C | C | C | **A/R** |
| Preparación de la exposición y evidencias | R | R | R | **A** |
| Verificación técnica de API y seguridad | **A/R** | C | C | I |
| Verificación de esquema MySQL | C | **A/R** | I | I |
| Verificación de interfaz Angular | C | I | **A/R** | I |

La gerencia real debe validar reglas de negocio antes de considerarlas aceptadas. **Aarón gestiona la presentación y el proceso; cada desarrollador explica su propia implementación.** La fila de verificación técnica asigna la responsabilidad exclusivamente dentro de la especialidad de cada integrante.

## Riesgos y bloqueos iniciales

| Riesgo | Mitigación |
| --- | --- |
| Cambios no coordinados de contratos y esquema | Acordar DTO/campos y usar PR con revisores |
| Roles/autorizaciones incompletos | Matriz de permisos, pruebas negativas |
| Datos sensibles en repo público | Datos sintéticos, protección de secretos |
| Falta de hosting para API .NET | Validación de entorno antes de liberar |
| Tablero engañoso por estados incorrectos | Marcar solo tareas realmente iniciadas |
