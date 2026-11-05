# Gestión del proyecto — Scrum con tablero Kanban

## Objetivo de producto

Ofrecer un portal público para consultar servicios de motocicletas y módulos privados que permitan gestionar clientes, unidades, kilometraje, citas y mantenimiento con control de acceso verificable.

## Marco de trabajo

Se utilizarán **sprints de dos semanas como cadencia propuesta** y **Kanban como control visual del flujo** (enfoque de Scrum con Kanban). El equipo debe registrar cuándo inicia realmente cada sprint; no se atribuyen ceremonias ni entregas que aún no hayan ocurrido.

### Responsabilidades de Scrum y roles técnicos

| Responsabilidad | Propuesta para SSC | Atribución práctica |
| --- | --- | --- |
| Product Owner (PO) | Kort como interlocutor temporal; validación funcional con gerencia/cliente y docente | Mantener el Product Backlog ordenado y negociar valor; no fingir aceptación del negocio |
| Scrum Master (SM) | Aarón como facilitador propuesto | Facilitar las reuniones, visibilizar impedimentos y mantener transparencia |
| Developers (equipo Scrum) | Kort, Gael, Vixtor Fernando y Aarón | Producir incremento integrado: código, datos, pruebas y documentación |

**Validación pendiente:** el equipo debe confirmar en un acta los cargos propuestos de PO y SM. La división de especialidades sigue vigente: Kort backend/integración, Gael datos, Fernando frontend, Aarón documentación/analítica.

### Eventos por sprint (propuesta)

| Evento | Cadencia | Evidencia GitHub |
| --- | --- | --- |
| Sprint Planning | Inicio del sprint | Sprint Goal, Issues seleccionados, capacidad y dependencias |
| Daily Scrum | Diario, presencial o mensaje breve | Obstáculos y ajustes en Project; no inventar asistencias |
| Refinamiento | Durante sprint | Historias con criterios, puntos y dependencias revisados |
| Sprint Review | Final | Demo verificable, PR, resultados de pruebas, observaciones |
| Retrospectiva | Final | Discussion/acta y una mejora concreta para el próximo ciclo |

### Kanban: definición explícita de flujo

**Backlog → Preparado → En desarrollo → En revisión → Pruebas → Terminado.**

- **Backlog:** idea o US documentada sin compromiso de sprint.
- **Preparado:** cumple Definition of Ready (DoR) y tiene prioridad, dependencia y criterios claros.
- **En desarrollo:** implementándose activamente en rama/PR.
- **En revisión:** PR abierto con CI y revisor asignado; no confundir PR abierto con aprobado.
- **Pruebas:** revisión técnica aprobada y pruebas funcionales/seguridad por ejecutar o validar.
- **Terminado:** cumple Definition of Done (DoD), está integrado y la evidencia es accesible.
- **Bloqueado:** marcar y explicar causa en la tarjeta; no cuenta como terminado.

**Límites WIP iniciales propuestos:** En desarrollo = máximo 3 tareas en todo el equipo, En revisión = 2, Pruebas = 2. Son políticas revisables al medir el flujo; no significan que tres personas deban estar ocupadas a la vez.

### Definition of Ready

Actor y valor identificados; prioridad y puntos estimados; criterios de aceptación reproducibles; dependencias aclaradas; permisos, campos, datos y contrato API conocidos cuando correspondan; tamaño razonable para el sprint.

### Definition of Done

Criterios aceptados y comprobados; pruebas pertinentes en verde y evidencia vinculada; revisión de otra persona; documentación/diagramas/contratos actualizados; errores críticos resueltos; sin secretos ni datos privados; decisión funcional registrada cuando corresponda.

### Métricas

WIP, throughput (elementos terminados), cycle time (inicio a fin) y work item age (edad de un elemento abierto). Los puntos no miden desempeño individual y el burndown solo aplica si realmente se registran las iteraciones.

## Configuración esperada en GitHub Projects

- Proyecto: [Follow Motors - SSC](https://github.com/orgs/iokort/projects/1)
- Vista **Kanban**: agrupar por campo Status usando el flujo anterior.
- Vista **Product Backlog**: tabla ordenada por prioridad y dependencias.
- Vista **Sprint**: filtrar por campo Iteration del sprint activo.
- Campos: Status, Iteration, Prioridad (Alta/Media/Baja), Puntos (Number), Épica (Single select), Assignees, Linked PR.
- Los Issues son unidades de trabajo; Discussions no sustituyen Issues.
- Nueva US entra a Backlog; asignar al sprint solo cuando se confirme capacidad. No poner todo automáticamente En desarrollo.

## Referencias

Schwaber, K., & Sutherland, J. (2020). *The Scrum guide*. https://scrumguides.org/scrum-guide.html

Coleman, J., & Vacanti, D. S. (2025). *The Kanban guide* (edición mayo de 2025). Kanban Guides. https://kanbanguides.org/the-kanban-guide
