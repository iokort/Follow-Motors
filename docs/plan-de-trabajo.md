# Plan de trabajo

## Fase 0 — Contratos
- **Aarón:** documentar requisitos, actores, casos de uso y criterios de aceptación por módulo.
- **Víctor:** inventariar pantallas, campos, acciones y estados (cargando, vacío, error, éxito).
- **Gael:** proponer ER, diccionario de datos, llaves e integridad según requisitos.
- **Kort:** validar contratos HTTP, roles, flujo de datos e infraestructura.

## Fase 1 — Primer corte vertical
Elegir el primer caso de uso con el equipo. Construir un flujo completo (DB + API + frontend + documentación), validarlo con datos ficticios y fusionarlo mediante PR. Evitar que cada área desarrolle todas sus piezas aisladas antes de integrar.

## Fase 2 — Resto de módulos
Avanzar por funcionalidades terminadas. Revisar dependencias entre cuentas, unidades, kilometraje, servicios y garantías. Mantener contratos API y migraciones versionados.

## Fase 3 — Calidad y producción
Pruebas, seguridad, roles, respaldos, documentación final y publicación controlada desde `main` tras aprobación.

## Definition of Done
Issue con criterios satisfechos, cambios probados, documentación sincronizada, sin secretos, CI y revisión de otra persona, integración en `develop`. Para releases, además comprobación de regresión e instrucciones de despliegue/rollback.
