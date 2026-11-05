# Registro de decisiones arquitectónicas (ADR)

Cada decisión técnica importante se documenta en un archivo `NNNN-descripcion.md` con:

- Estado: Propuesta / Aceptada / Sustituida.
- Contexto y problema.
- Alternativas consideradas y criterios.
- Decisión y motivos.
- Consecuencias, riesgos y fecha de aprobación.
- Issues y PR relacionados.

## ADR iniciales pendientes de validar

| ID | Decisión | Estado |
| --- | --- | --- |
| ADR-0001 | Separar web pública y recursos privados con API y autorización en servidor | Propuesta |
| ADR-0002 | MySQL relacional y migraciones versionadas | Propuesta |
| ADR-0003 | Angular con separación MVVM inspirada; backend ASP.NET Core modular | Propuesta |
| ADR-0004 | Estrategia de hosting y despliegue con rollback | Pendiente |
| [ADR-0006](0006-angular-frontend.md) | Angular 22 en frontend, ASP.NET Core C# y MySQL | Decisión tecnológica |
| [ADR-0005](0005-alta-presencial-posventa.md) | Alta presencial y portal exclusivo de asistencia | Decisión vigente; implementación pendiente |

Los ADR no son acuerdos aceptados hasta que conste revisión y responsable de decisión.
