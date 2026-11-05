# Expediente académico y técnico — Follow Motors

Este índice presenta los entregables de ingeniería de software para revisión pública por el docente y el equipo. **Diseño documentado** no equivale a **función implementada**.

## Guía de navegación

| Artefacto | Archivo | Naturaleza |
| --- | --- | --- |
| **Alcance vigente de posventa** | [Reglas de producto](alcance-posventa.md) · [ADR-0005](adr/0005-alta-presencial-posventa.md) | Prioridad sobre planificación anterior |
| Problema, objetivos y alcance | [Marco teórico y problema](marco-teorico.md) | Análisis |
| Metodología y roles Scrum + Kanban | [Guía de trabajo](scrum-kanban.md) | Propuesta de operación |
| Product backlog con 22 US | [Historias y priorización](backlog-v5.md) | Línea base V5; revalidar en refinamiento |
| Diseño orientado a objetos | [15 CRC base](tarjetas-crc.md) · [180 fichas contextuales](crc/README.md) | Ampliación de 9 historias, diseño propuesto |
| [Diagramas de casos de uso, clases y secuencias](diagramas/README.md) | Modelos propuestos | Referencia para revisión | 
| **Stack definitivo: Angular + C# + MySQL** | [ADR-0006](adr/0006-angular-frontend.md) | Decisión técnica |
| Arquitectura Angular y MVVM | [Arquitectura de presentación y API](mvvm-y-arquitectura.md) | Diseño, no implementación terminada |
| Equipo, cargos y exposición | [Responsabilidades](equipo-y-roles.md) · [RACI](stakeholders-raci.md) | Gobierno del trabajo |
| Actores y RACI | [Stakeholders y responsabilidades](stakeholders-raci.md) | Análisis |
| Plan y fases técnicas | [Plan existente](plan-de-trabajo.md) | Planificación |
| Contratos API | [Especificación](api.md) | Contratos iniciales |
| Inventario funcional legado | [Módulos](modulos.md) | Referencia |
| Evidencias, revisión y aceptación | [Calidad y trazabilidad](trazabilidad-y-evidencias.md) | Proceso |
| Registro de decisiones | [ADRs](adr/README.md) | Decisiones y pendientes |

## Dónde está cada cosa en GitHub

- [Código y documentación](https://github.com/iokort/Follow-Motors)
- [Issues y backlog](https://github.com/iokort/Follow-Motors/issues)
- [Tablero SSC](https://github.com/orgs/iokort/projects/1)
- [Pull requests y revisiones](https://github.com/iokort/Follow-Motors/pulls)
- [Discussions para acuerdos](https://github.com/iokort/Follow-Motors/discussions)
- [Actions y pruebas](https://github.com/iokort/Follow-Motors/actions)
- [Security](https://github.com/iokort/Follow-Motors/security)

## Estado comprobado en la rama de partida

La base técnica contiene una API ASP.NET Core para comprobar salud, una conexión MySQL local, un frontend Angular inicial, CI y CodeQL. **No se consideran desarrollados** los módulos de negocio, el esquema relacional definitivo ni la autenticación completa. Los documentos describen el diseño esperado.

## Fuente de planificación

Se tomó como línea base el documento interno *Follow Motors: planificación integral, versión 5.0* (30 de octubre de 2026), de 62 páginas. Los 22 identificadores US, estimaciones y 15 propuestas CRC proceden de esa planificación; las prioridades y estimaciones pueden cambiar tras la revisión del equipo.

[Publicación GitHub](github-publicacion.md) · [Plantilla Wiki Home](wiki/Home.md).
