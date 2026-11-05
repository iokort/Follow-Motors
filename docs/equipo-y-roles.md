# Equipo, roles y responsabilidades

## Organización de trabajo

| Integrante / GitHub | Rol del proyecto | Entrega principal | Decisiones y revisión |
| --- | --- | --- | --- |
| Kort · @kortCobein | Líder técnico e integrador de sistemas | Backend C#, API, integración frontend/API, arquitectura y releases | Define contratos con el equipo; revisa integración y cambios entre áreas |
| Gael · @G43LEX | Responsable de datos / ingeniero de base de datos | Modelo ER, MySQL, migraciones, índices, integridad y datos de prueba | Revisa cualquier cambio de esquema o consulta relevante |
| Vixtor Fernando · @fernandoohiti25 | Responsable de frontend | Angular, TypeScript, componentes y CSS, interfaz responsive, accesibilidad y consumo de API | Revisa cambios de interfaz y acuerda contratos JSON con Kort |
| Aarón · @Aaron262005 | Analista funcional y responsable de documentación | Requisitos, casos de uso, criterios de aceptación, manuales y registro de decisiones | Revisa documentación y coteja requisitos con resultados, sin suplantar pruebas técnicas |

## Autoridad y decisiones

- Cada responsable decide la implementación de su área dentro de los contratos aprobados.
- Las modificaciones entre áreas (campos, JSON, errores, migraciones) se acuerdan entre sus responsables **antes** del cambio.
- Kort coordina el ensamblaje, pero la revisión se realiza entre pares; nadie aprueba su propio PR.
- Aarón documenta los acuerdos y mantiene trazabilidad entre Issues, requisitos, pruebas y manuales.
- El equipo valida la aceptación de una funcionalidad completa antes de llevarla a `main`.

**Importante:** GitHub tiene roles de acceso al repositorio (Read, Triage, Write, Maintain, Admin), distintos de los roles funcionales de este documento. El equipo de desarrollo debe tener permiso `Write`; la administración del repositorio queda reservada a quienes realmente la tengan.

## Responsabilidad técnica por carpeta

- `backend/`: Kort (code review cruzada por Fernando cuando se modifiquen contratos frontend).
- `database/`: Gael y Kort.
- `frontend/`: Vixtor Fernando y Kort.
- `docs/`: Aarón y Kort.
- `.github/` y despliegue: Kort y Aarón para documentación de procesos.

`CODEOWNERS` solicita revisiones pero no bloquea a otros colaboradores para leer o proponer cambios.

## Cómo medir avances

Cada tarea debe tener: responsable, alcance, criterio de aceptación, dependencias, rama, PR vinculado y pruebas. La comprobación técnica de un cambio no equivale a la aceptación funcional del módulo.
