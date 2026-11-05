# Equipo SSC — Roles de gestión y desarrollo

## Organización funcional del proyecto

| Integrante | Cargo del equipo | Responsabilidad verificable | Presenta en la exposición |
| --- | --- | --- | --- |
| **Aarón** · @Aaron262005 | **Project Manager y responsable de documentación** | Coordina hitos, prioridades acordadas, reuniones, impedimentos, entregables, evidencias, documentación y trazabilidad en GitHub Projects | Objetivo del proyecto, alcance, plan, Scrum/Kanban, avance real, riesgos y evidencias |
| **Gael** · @G43LEX | **Database Manager** y referente del enfoque de producto | Propone la dirección funcional y reglas del negocio para discutirlas con el equipo; diseña modelo ER, MySQL, migraciones, integridad y consultas | Por qué se construye así, reglas de negocio, diseño relacional y persistencia |
| **Vixtor Fernando** · @fernandoohiti25 | **Frontend Designer & Developer** | Diseña e implementa la interfaz con Angular, TypeScript y CSS; UX, navegación, accesibilidad e integración de servicios HTTP | Maquetas, componentes Angular, estado de interfaz y experiencia del cliente |
| **Kort** · @kortCobein | **Backend Developer** | Implementa API C# / ASP.NET Core, autenticación y autorización, casos de uso, validaciones y contratos con el frontend; coordina la integración técnica necesaria | Endpoints, principios SOLID aplicados, lógica de negocio en servidor, seguridad e integración |

## Qué significa «dirigir el proyecto»

El **Project Manager** coordina **el proceso de trabajo**, no sustituye la autoridad de los especialistas sobre su código. Aarón registra acuerdos, planifica revisiones, mantiene el tablero y comprueba que cada entrega tenga responsable y evidencia. No necesita explicar detalles de POO o SQL que corresponden a otros integrantes.

Gael aporta la **visión funcional original** y es el referente del enfoque de producto. Sus propuestas se documentan como decisiones a validar; eso no significa que una persona ajena a la empresa pueda aprobar unilateralmente políticas de clientes o garantías.

Kort se encarga de la integración técnica para conectar backend, Angular y MySQL, **sin ocupar el cargo de Project Manager**. Fernando es responsable de la solución visual y su implementación. Cada integrante responde por la calidad y decisiones de su área.

## Gobierno de decisiones

- **Organización, calendario, impedimentos y seguimiento:** Aarón coordina y registra en Projects, Issues, Discussions y actas.
- **Alcance, prioridades y enfoque funcional:** Gael propone y el equipo acuerda; la aceptación de reglas comerciales reales requiere aprobación de la empresa.
- **Contratos JSON, autorización y servidor:** Kort define y coordina con Fernando y Gael antes de cambios incompatibles.
- **Diseño visual e implementación Angular:** Fernando lidera las decisiones de presentación.
- **Esquema, integridad y migraciones MySQL:** Gael lidera las decisiones de persistencia.
- **Revisión:** PR con revisor distinto de su autor, comprobaciones automáticas y criterios de aceptación antes de integrar.

## Responsables técnicos por carpeta

- `backend/` — Kort.
- `database/` — Gael.
- `frontend/` — Vixtor Fernando.
- `docs/` — Aarón, con aportaciones y revisión técnica de cada especialista.
- `.github/` y despliegue — coordinación técnica de Kort; Aarón registra el proceso y evidencias.

`CODEOWNERS` configura revisores por carpetas; los cargos académicos no son los roles de permiso de GitHub (Read, Write, Maintain, Admin).

## Evidencias que corresponden a cada cargo

Aarón: tablero actualizado, backlog ordenado, actas reales, definición de alcance, trazabilidad y reporte de riesgos. Gael: diagrama ER, scripts/migraciones y verificaciones de integridad. Fernando: interfaz Angular, PR, pruebas y demostración. Kort: API, contratos, pruebas, seguridad e integración.

**No atribuir a Aarón código que no desarrolló ni presentar como concluidas reuniones, funciones o pruebas que no ocurrieron.** Los cargos sirven para repartir la exposición y deben sostenerse con trabajo real.

[Kanban SSC](https://github.com/orgs/iokort/projects/1) · [Plan](plan-de-trabajo.md) · [RACI](stakeholders-raci.md)
