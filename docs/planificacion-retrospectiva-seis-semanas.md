# Matriz retrospectiva de planificación — Follow Motors

> **Naturaleza del registro:** reconstrucción de actividades por semana para revisar y asignar; no constituye evidencia de ejecución en esas fechas ni atribución verificada de trabajo. Las fechas indican ventanas de planificación, no fechas de commits reales.

> Las actividades son propuestas vinculadas al Project Charter. Para marcar una como completada se requiere evidencia de implementación, revisión o prueba.

| Semana | Periodo | Responsable | Aportación A | Aportación B | Estado |
|---|---|---|---|---|---|
| 1 | 2026-10-15 al 2026-10-21 | Kort (Backend e integración) | Diseñar contratos API REST y autenticación | Definir límites de Domain/Application/Infrastructure/Presentation | Por validar |
| 1 | 2026-10-15 al 2026-10-21 | Gael (Datos y MySQL) | Identificar entidades del taller | Modelar clientes, usuarios y motocicletas | Por validar |
| 1 | 2026-10-15 al 2026-10-21 | Fernando (Frontend e interfaz) | Identificar pantallas por rol | Diseñar navegación de acceso y sesión | Por validar |
| 1 | 2026-10-15 al 2026-10-21 | Aarón (Requisitos y calidad) | Analizar alcance del Project Charter | Organizar RF-01 a RF-06 | Por validar |
| 2 | 2026-10-22 al 2026-10-28 | Kort (Backend e integración) | Diseñar comandos y consultas CQRS | Proponer validaciones de entidades y casos de uso | Por validar |
| 2 | 2026-10-22 al 2026-10-28 | Gael (Datos y MySQL) | Relacionar franjas y citas | Definir restricciones de integridad de expedientes | Por validar |
| 2 | 2026-10-22 al 2026-10-28 | Fernando (Frontend e interfaz) | Especificar formularios de clientes | Especificar formulario de motocicletas | Por validar |
| 2 | 2026-10-22 al 2026-10-28 | Aarón (Requisitos y calidad) | Definir historias de usuario y criterios | Crear matriz inicial de trazabilidad | Por validar |
| 3 | 2026-10-29 al 2026-11-04 | Kort (Backend e integración) | Estructurar servicios de persistencia MySQL | Diseñar sesión JWT y protección por roles | Por validar |
| 3 | 2026-10-29 al 2026-11-04 | Gael (Datos y MySQL) | Modelar atenciones y presupuestos | Diseñar tablas de órdenes, refacciones y garantías | Por validar |
| 3 | 2026-10-29 al 2026-11-04 | Fernando (Frontend e interfaz) | Diseñar calendario y gestión de citas | Definir estados de carga, error y vacío | Por validar |
| 3 | 2026-10-29 al 2026-11-04 | Aarón (Requisitos y calidad) | Revisar reglas operativas de cita y recepción | Definir casos de prueba de identidad | Por validar |
| 4 | 2026-11-05 al 2026-11-11 | Kort (Backend e integración) | Implementar acceso a datos parametrizado | Proteger endpoints y separar permisos | Por validar |
| 4 | 2026-11-05 al 2026-11-11 | Gael (Datos y MySQL) | Preparar SQL de esquema y FKs | Revisar índices y validación referencial | Por validar |
| 4 | 2026-11-05 al 2026-11-11 | Fernando (Frontend e interfaz) | Definir vistas de servicios y garantías | Diseñar resumen administrativo | Por validar |
| 4 | 2026-11-05 al 2026-11-11 | Aarón (Requisitos y calidad) | Revisar aceptación de servicios y garantías | Organizar artefactos Scrum y evidencias | Por validar |
| 5 | 2026-11-12 al 2026-11-18 | Kort (Backend e integración) | Implementar flujo de citas transaccional | Preparar endpoints de clientes y motocicletas | Por validar |
| 5 | 2026-11-12 al 2026-11-18 | Gael (Datos y MySQL) | Comprobar compatibilidad entre entidades y API | Revisar transacciones y control de aforo | Por validar |
| 5 | 2026-11-12 al 2026-11-18 | Fernando (Frontend e interfaz) | Precisar contratos HTTP con backend | Diseñar guards por rol e interceptor JWT | Por validar |
| 5 | 2026-11-12 al 2026-11-18 | Aarón (Requisitos y calidad) | Documentar riesgos de seguridad y SQL | Preparar plan de pruebas funcionales | Por validar |
| 6 | 2026-11-19 al 2026-11-26 | Kort (Backend e integración) | Integrar módulos de servicios, garantías e indicadores | Compilar backend con GitHub Actions y corregir errores | Por validar |
| 6 | 2026-11-19 al 2026-11-26 | Gael (Datos y MySQL) | Revisar consultas para reportes | Documentar pruebas pendientes de persistencia | Por validar |
| 6 | 2026-11-19 al 2026-11-26 | Fernando (Frontend e interfaz) | Preparar integración Angular/Ionic con API | Verificar accesibilidad y compilación frontend | Por validar |
| 6 | 2026-11-19 al 2026-11-26 | Aarón (Requisitos y calidad) | Revisar aceptación por módulo y responsables | Consolidar estado real de entregables | Por validar |

## Evidencia técnica confirmada

- Implementación real publicada el 26/11/2026 en la rama `main`: solución .NET 10 por capas, contratos CQRS, acceso MySQL, endpoints REST y autenticación JWT.
- Build backend en GitHub Actions: exitoso para el commit `48c354390856cee97939b3c4412850db7fbbedb8`.
- No se ha verificado todavía integración con una base MySQL activa, frontend Angular/Ionic ni ejecución completa de pruebas funcionales.

## Uso en Jira

Usar esta matriz para descomponer y asignar actividades, consignando fecha de registro real. No alterar fechas de creación, autorías ni cierres sin evidencia.
