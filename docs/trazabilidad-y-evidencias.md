# Trazabilidad, pruebas y evidencias

## Cadena de trazabilidad

Cada entrega verificable debe poder seguirse así:

`USxx → RFxx / RNFxx → Issue → Rama → PR → código/esquema → caso de prueba CP-xx → resultado → estado en Project`

El tablero público es una vista de avance, no prueba suficiente de que una funcionalidad funciona.

## Plantilla de evidencia por historia

| Campo | Evidencia esperada |
| --- | --- |
| Historia y alcance | USxx, actor, beneficio y prioridad |
| Criterios de aceptación | AC1, AC2, AC3 con resultados |
| Diseño | CRC, diagrama, ADR o mockup vinculado cuando aplique |
| Código | PR y archivos modificados |
| Datos | Migración/consultas e integridad comprobadas |
| Pruebas | Unidad, integración, negativas de permisos y UI según corresponda |
| Revisión | Persona distinta del autor |
| Resultado | Aceptada / rechazada / pendiente, fecha y causa |

## Control de calidad por módulo

1. Asegurar que la ruta pública funcione sin sesión cuando corresponde.
2. Para recursos privados, probar usuario autorizado y usuario ajeno.
3. Verificar errores de validación, de red y estados vacíos.
4. Validar integridad y atomicidad de cambios de kilometraje, citas y mantenimiento.
5. Ejecutar compilación .NET, CI frontend y CodeQL.
6. Guardar decisiones y aprobación de criterios pertinentes.

## Registro de sprints

Una carpeta `docs/sprints/` podrá reunir por sprint: objetivo, Issues seleccionados, fecha real de inicio/cierre, resumen Review, resultados de la demo, Retrospective y mejora siguiente. No generar actas o capturas ficticias.
