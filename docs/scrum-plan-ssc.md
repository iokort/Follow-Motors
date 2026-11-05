# Plan de trabajo Scrum — Equipo SSC

**Propósito:** demostrar para Follow Motors seguimiento de garantías, agenda de citas y panel del cliente. **Plan propuesto**, no acta de sprints ya realizados ni evidencia de funciones productivas.

## Equipo y responsabilidades

| Persona | Cargo de exposición | Labor en cada incremento |
| --- | --- | --- |
| Aarón (@Aaron262005) | **Project Manager** | Mantiene backlog, tablero, criterios, acuerdos, manual, riesgos y evidencias; facilita reuniones |
| Gael (@G43LEX) | **Database Manager** y referente de producto | Modela persistencia MySQL V3, propone prioridades y aclara reglas |
| Vixtor Fernando (@fernandoohiti25) | **Frontend Designer & Developer** | Mantiene su diseño existente y lo evoluciona hacia Angular 22 / MVVM |
| Kort (@kortCobein) | **Backend Developer** | API ASP.NET Core .NET 10, SOLID, permisos y conexión al frontend |

**Scrum:** `Project Manager` es un cargo del equipo escolar, **no un rol de la Guía Scrum**. Para la metodología se necesita que alguien asuma la responsabilidad de ordenar el Product Backlog (referente propuesto: Gael), que Aarón facilite el proceso (sin atribuirle certificación de Scrum Master) y que los cuatro participen en la entrega. La empresa/gerencia valida reglas operativas cuando corresponda.

## Cómo trabajaremos, sin reuniones ficticias

- **Cadencia propuesta:** sprints de **una semana lectiva**, ajustables según carga escolar. Aarón registra fechas reales cuando el grupo acuerde comenzar.
- **Planning (20–30 min):** seleccionar US dependientes ya preparadas, comprometer objetivo pequeño, confirmar quién hace qué.
- **Seguimiento (5–10 min, 2–3 veces por semana):** cada integrante informa avance, bloqueo y próximo paso; Aarón actualiza Projects.
- **Review (20 min):** demo real de un recorrido frontend → API → MySQL o de documentación verificable.
- **Retrospectiva (10 min):** identificar una cosa que mejorar; registro de acuerdo simple, sin ceremonias simuladas.
- **Refinamiento:** cuando cambie una regla de negocio, actualizar la US, la decisión y sus criterios antes del código.

## Orden de construcción por dependencias

| Iteración sugerida | Objetivo verificable | US principales | Responsables | Evidencia esperada |
| --- | --- | --- | --- | --- |
| **Preparación** | Clonar, compilar y entender frontend de Fernando + SQL V3 de Gael | Base técnica | Los cuatro | CI, README, diagrama ER y contratos acordados |
| **Sprint 1** | Identidad y expedientes que permitan operar con motos propias | US21, US01, US02, US04 | Kort + Gael + Fernando; Aarón documenta | Cuentas sintéticas, permisos, alta sin autorregistro |
| **Sprint 2** | Agenda interna para CFMOTO y motos de otras marcas | US06, US15 y base US05 | Gael + Kort + Fernando; Aarón coordina | Cita creada/cambiada en DB desde interfaz |
| **Sprint 3** | Seguimiento y decisiones reales de garantía | US07, US23, US16 | Gael + Kort + Fernando; Aarón registra pruebas | Servicios, atrasos, decisiones y cierre trazables |
| **Sprint 4** | Portal cliente y demo conjunta | US08, US13 y ajustes de US02 | Fernando + Kort + Gael; Aarón prepara exposición | Usuario ve únicamente sus motos/garantías; recorrido completo |

**No se asume que estas iteraciones ya se realizaron.** El alcance público complementario (US09–US12 y US14) puede demostrarse con la interfaz existente si no interfiere con los tres módulos priorizados.

## Flujo real del GitHub Project

[Tablero de trabajo SSC](https://github.com/orgs/iokort/projects/1) emplea las opciones existentes:

`Sin Iniciar → Ready → Working → Waiting → Done`; `Canceled` para trabajo descartado.

- **Sin Iniciar:** historia documentada, aún sin aceptar para sprint.
- **Ready:** dependencia y criterio aceptados, contrato definido si procede.
- **Working:** alguien está desarrollándola activamente.
- **Waiting:** falta revisión, respuesta, dependencia o prueba; anotar el bloqueo.
- **Done:** criterio validado, PR integrado y evidencia registrada.
- **Canceled:** fuera de alcance de la entrega y decidido explícitamente, sin borrar historia.

**WIP sugerido:** máximo dos historias activas a la vez, más las revisiones de PR; no asignar 12 US simultáneas a cuatro alumnos.

## Definition of Ready (DoR)

Historia redactada como actor-necesidad-beneficio; reglas y excepción conocidas; alcance que puede terminarse en un sprint; usuario y dependencia claros; responsables y puntos estimados en refinamiento.

## Definition of Done (DoD)

Código/documentación coherente con la historia; pruebas realmente ejecutadas; CI verde; revisión cruzada del área correspondiente; seguridad y permisos verificados; guía y CRC actualizadas si cambió la arquitectura; Aarón enlaza evidencia en Issue/PR.

## Seguimiento y entregables para exposición

| Evidencia | Responsable primario | Cómo demostrar |
| --- | --- | --- |
| Objetivo, backlog, cronograma y riesgos | Aarón | Issues + Projects + actas reales |
| Diagrama ER y SQL V3 | Gael | Scripts versionados, consultas de verificación |
| Angular, estilos, vistas y MVVM | Fernando | Pantallas reales y código; migración incremental, no fingir que todo es Angular nativo |
| C#/.NET, SOLID, API y seguridad | Kort | Endpoints y pruebas contra MySQL |
| CRC, historias y decisiones | Todos; Aarón organiza | 4 épicas, 12 US, 144 CRC, PR por cambio |
| Demostración integrada | Todos | Recorrido de alta → cita → servicio/garantía → consulta cliente |

## Riesgos y acciones

| Riesgo | Acción |
| --- | --- |
| Fechas de servicio ambiguas (3/6/12) | Decidir con Gael si son meses desde venta o tras servicio anterior; no cambiar SQL por intuición |
| Trabajo de Fernando sobrescrito | Mantener sus vistas, clases y estilos; adaptar al shell Angular antes de portar componentes |
| Datos de Gael sin migración ejecutada | Probar scripts V3 en MySQL local de pruebas, nunca actualizar producción sin respaldo |
| Panel cliente filtra solo visualmente | Validar titularidad obligatoriamente en API C# |
| Docente pide evidencia de código | Cada desarrollador presenta sus archivos reales y pruebas, Aarón la metodología |

## Referencias

Schwaber, K., & Sutherland, J. (2020). *The Scrum guide*. https://scrumguides.org/scrum-guide.html

[Backlog por épicas](epicas/README.md) · [Stakeholders](stakeholders-ssc.md) · [Políticas de contribución](../CONTRIBUTING.md).

## Entrega de backend en revisión — US06 y US15

**PR #48:** capa C# con Controller, servicios SOLID, persistencia y pruebas para creación/administración de citas. El esquema SQL es obra de Gael y el diseño de pantallas sigue siendo de Fernando. El incremento puede revisarse como **backend**, pero no se considera una demostración de las historias completas hasta contar con credenciales de desarrollo, frontend conectado y recorrido real. Aarón puede enlazar PR, scripts y resultados de Actions como evidencia **cuando estén aprobados**.
