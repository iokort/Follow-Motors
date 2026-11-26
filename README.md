# Follow Motors

Sistema de agenda y control de servicios para motocicletas. El alcance acordado está descrito en el Project Charter; el repositorio conserva el diseño y código del equipo y Jira concentra la planificación, criterios y evidencias.

## Alcance vigente

| Requisito | Jira |
| --- | --- |
| RF-01 · Autenticación y permisos | [SCRUM-5](https://iokort.atlassian.net/browse/SCRUM-5) |
| RF-02 · Clientes y motocicletas | [SCRUM-6](https://iokort.atlassian.net/browse/SCRUM-6) |
| RF-03 · Citas | [SCRUM-7](https://iokort.atlassian.net/browse/SCRUM-7) |
| RF-04 · Mantenimiento y garantías | [SCRUM-8](https://iokort.atlassian.net/browse/SCRUM-8) |
| RF-05 · Reportes | [SCRUM-9](https://iokort.atlassian.net/browse/SCRUM-9) |
| RF-06 · Recordatorios | [SCRUM-10](https://iokort.atlassian.net/browse/SCRUM-10) |

## Arquitectura prevista

Angular/TypeScript e Ionic para la interfaz web adaptable; API REST en C#/ASP.NET Core, CQRS y MySQL. Los recordatorios por WhatsApp dependen de las condiciones del canal. Los scripts y el diagrama de datos se encuentran en `database/`; las historias y reglas de negocio existentes en `readmis/`. La presencia de una historia o una rama no equivale a una implementación terminada.

## Equipo

| Responsable | Área |
| --- | --- |
| @kortCobein | Backend, API e integración |
| @G43LEX | Datos, MySQL e integridad |
| @fernandoohiti25 | Frontend e interfaz |
| @Aaron262005 | Requisitos, calidad y documentación |

## Colaboración

- [Tablero Scrum](https://iokort.atlassian.net/jira/software/projects/SCRUM/boards/1) · [Backlog](https://iokort.atlassian.net/jira/software/projects/SCRUM/boards/1/backlog)
- [Guía de contribución](CONTRIBUTING.md) · [Pull requests](https://github.com/iokort/Follow-Motors/pulls)

Jira es la fuente de seguimiento; GitHub conserva código, revisiones e historial técnico. La reconciliación de las ramas `main` y `develop` se controla en [SCRUM-2](https://iokort.atlassian.net/browse/SCRUM-2).
