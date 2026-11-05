# Stakeholders y matriz RACI — Follow Motors SSC

Los *stakeholders* son las personas o grupos afectados por el sistema o con capacidad de validar requisitos. **No son sinónimo de los cuatro autores del código.** Este cuadro es una identificación académica: no implica entrevistas o aprobaciones que no hayan ocurrido.

## Stakeholders del producto

| Clave | Interesado | Qué necesita / aporta | Influencia | Interacción prevista |
| --- | --- | --- | --- | --- |
| ST01 | Gerente / asesor de Follow Motors | Alta presencial, credenciales, agenda, correcciones y decisión de garantías | Alta | Define y valida las reglas operativas |
| ST02 | Clientes propietarios habilitados | Ver únicamente sus unidades, historial y garantía | Alta en usabilidad | Pruebas con cuentas sintéticas, sin exponer datos reales |
| ST03 | Mecánicos del taller | Registrar trabajos y kilometraje para revisión de gerencia | Media | Validar datos de una orden de mantenimiento |
| ST04 | Clientes de motos de otra marca | Solicitar por WhatsApp una cita sin crear cuenta/moto de portal | Media | Verificar el proceso de atención EXTERNA |
| ST05 | Equipo SSC (4 estudiantes) | Entrega funcional y demostrable, sin dependencias innecesarias | Alta en desarrollo | Diseño, desarrollo, pruebas y exposición |
| ST06 | Docente / evaluador | Rúbrica, evidencia de POO/SOLID, historias, CRC y Scrum | Alta en evaluación | Revisión de repositorio y demo real |
| ST07 | Administración de la empresa | Privacidad, seguridad y conservación de expedientes | Alta | Validación si el sistema se usara con usuarios reales |
| ST08 | Encargado de infraestructura | Hosting .NET, copias de seguridad, despliegue y recuperación | Media | Prueba antes de publicar |
| ST09 | Visitantes del sitio | Información de servicios y contacto; nunca venta en línea | Baja | Acceso público informativo |

## Equipo y autoridad

| Integrante | Cargo ante el docente | Decisiones y responsabilidades |
| --- | --- | --- |
| Aarón (@Aaron262005) | Project Manager | Coordinación, backlog, evidencias, documentación y seguimiento |
| Gael (@G43LEX) | Database Manager | Enfoque funcional acordado, esquema MySQL V3, índices, migraciones |
| Vixtor Fernando (@fernandoohiti25) | Frontend Designer & Developer | UX, UI, navegación y evolución de su frontend hacia Angular/MVVM |
| Kort (@kortCobein) | Backend Developer | API ASP.NET Core C#, seguridad, contratos, integración |

**Dirección de proyecto vs. dirección técnica:** Aarón coordina entregables; Gael es referente del enfoque; Fernando decide la implementación de su frontend; Kort la API y la integración. Ninguno debe atribuirse trabajo ajeno. Las decisiones de gerencia empresarial no se fingen como aprobadas.

## RACI — responsabilidades comprobables

R = ejecuta; A = responsable final; C = consultado; I = informado. Un A por actividad.

| Actividad | Aarón PM | Gael DB | Fernando Front | Kort Back |
| --- | --- | --- | --- | --- |
| Plan, Issues, riesgos y seguimiento Scrum | **A/R** | C | C | C |
| Priorización y propuesta de reglas del producto | R | **A/R** | C | C |
| Diagramas ER, estructura y SQL de MySQL | I | **A/R** | C | C |
| Diseño de pantallas, Angular y MVVM | I | C | **A/R** | C |
| API REST C#, validaciones y autenticación | I | C | C | **A/R** |
| Contratos Angular ↔ API ↔ MySQL | I | R | R | **A/R** |
| Historias, CRC, casos de uso y documentación | **A/R** | C | C | C |
| Revisión y demostración de trabajo propio | R | R | R | **A** |
| Evidencias de Scrum y exposición documental | **A/R** | C | C | C |

La última fila de demo asigna a Kort coordinación **técnica**, no autoría de entregables de Gael/Fernando. La revisión es cruzada y se respeta el trabajo del autor.

## Comunicación de decisiones

- El gerente/asesor es una sola persona operativa **por ahora**; la BD V3 conserva roles independientes para posibles ampliaciones.
- Peticiones del cliente por WhatsApp; la agenda solo se modifica desde perfil de personal.
- Una cita EXTERNA puede corresponder a otra marca, sin crear moto CFMOTO propia.
- Las decisiones de invalidez de garantía deben motivarse y auditarse.

[Reglas de negocio](reglas-negocio-ssc.md) · [Plan Scrum](scrum-plan-ssc.md) · [Mapa de historias](epicas/README.md).
