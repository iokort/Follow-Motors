# Follow Motors

Sistema web para gestión y seguimiento de unidades, clientes, servicios y garantías.

**Estado:** reconstrucción colaborativa. La versión actual en [iokort.com/follow-motors](https://iokort.com/follow-motors) es referencia funcional, no una dependencia ni un entorno de pruebas. No modificar producción sin autorización.

## Equipo y responsabilidades

| Responsable | Área | Entregables |
| --- | --- | --- |
| Kurt Cobain Vázquez Sánchez (Kort) | Backend e integración | API REST en C# / ASP.NET Core, conexión con frontend, contratos y coordinación |
| Gael | Base de datos | MySQL, modelo relacional, migraciones, índices, datos de prueba |
| Víctor | Frontend | HTML semántico, CSS responsive, JavaScript y consumo de API |
| Aarón | Documentación | Requisitos, casos de uso, arquitectura, manuales y seguimiento documental |

Cada integrante prueba y documenta su propio cambio. Las interfaces y el modelo de datos se acuerdan antes de programar funcionalidades dependientes.

## Tecnologías

- **Backend:** C# y ASP.NET Core Web API.
- **Base de datos:** MySQL; consultas parametrizadas y credenciales únicamente en configuración local/segura.
- **Frontend:** HTML, CSS y JavaScript sin framework obligatorio.
- **Principios:** SOLID aplicados con criterio (responsabilidad única, dependencias claras, separación de responsabilidades); evitar abstracciones innecesarias.
- **Integración:** JSON sobre HTTP(S), rutas bajo `/api/v1`.

## Estructura

```text
backend/                API C# (.NET), dominio, servicios y pruebas
frontend/               Interfaz HTML, CSS, JS y recursos
database/               Esquema, migraciones, seeds y consultas
docs/                   Requisitos, arquitectura, API, manuales
.github/                 Plantillas, automatización y revisiones
```

Las carpetas de inicio son convenciones de trabajo: no implican que ya exista una aplicación funcional.

## Ramas

| Rama | Uso | Responsable principal |
| --- | --- | --- |
| `main` | Versión estable, lista para publicar | Equipo; integración coordinada por Kort |
| `develop` | Integración de cambios revisados | Equipo |
| `feature/backend` | API, reglas de negocio, integración | Kort |
| `feature/database` | Modelo, consultas y migraciones MySQL | Gael |
| `feature/frontend` | Pantallas, estilos e interacciones | Víctor |
| `feature/docs` | Documentación y manuales | Aarón |

No trabajar directamente sobre `main` ni `develop`. Crear ramas específicas adicionales como `feature/frontend-login`, `fix/backend-auth` o `docs/manual-usuario` cuando convenga. Las ramas compartidas por área requieren actualizarse desde `develop` antes de abrir un PR. **No realizar force push** sobre `main` ni `develop`.

## Proceso de trabajo

1. Crear o elegir un **Issue** con objetivo, responsable y criterios de aceptación.
2. Actualizar la rama de trabajo a partir de `develop` y desarrollar únicamente el alcance del Issue.
3. Realizar commits pequeños y explicativos: `feat(frontend): crear formulario de acceso`, `fix(api): validar kilometraje`, `docs: actualizar casos de uso`.
4. Abrir un **Pull Request hacia `develop`**, asociar el Issue y describir archivos, pruebas y dependencias.
5. Obtener revisión de otra persona, resolver comentarios y comprobar que no existan conflictos antes de integrar.
6. Cuando la integración esté comprobada, abrir un PR de `develop` hacia `main` para la entrega.
7. Desplegar únicamente desde una versión aprobada de `main`. Nunca sincronizar desarrollo directamente con producción.

**Reglas de coordinación:** nadie modifica el área principal de otro sin acordarlo; toda modificación de contratos API o esquema MySQL se comunica antes de implementarse; frontend no accede a MySQL directamente; las migraciones no deben destruir datos sin revisión expresa; no subir contraseñas, tokens, archivos `.env`, respaldos reales ni datos personales. No se aprueban PR sin explicar cómo se verificaron.

## Contrato de integración

- Backend define rutas, métodos, códigos HTTP y estructuras JSON junto con frontend y base de datos.
- Frontend centraliza llamadas a la API y gestiona estados de carga, error y éxito; no depende de nombres de tablas.
- Base de datos provee migraciones versionadas y documentación de relaciones, claves e índices. Backend es el único consumidor directo.
- Documentación se mantiene sincronizada con los PR; ejemplos de solicitudes y respuestas deben reflejar lo implementado.
- Definir CORS, autenticación, autorización y validaciones en backend antes de conectar datos reales. No guardar contraseñas sin hashing seguro.

## Módulos funcionales de referencia

La versión anterior incluye: acceso y registro, vistas de cliente/gerente, cuentas de clientes, registro de unidades, actualización de kilometraje, agenda de servicios, primer/segundo/tercer servicio y garantías finalizadas. **Estas pantallas son referencia para levantar requisitos**; no se considera que su comportamiento esté validado, ni se promete reproducir errores o decisiones técnicas antiguas.

## Arranque

1. Clonar: `git clone https://github.com/iokort/Follow-Motors.git`.
2. Cambiar a integración: `git switch develop`.
3. Trabajar desde una rama propia y consultar los archivos `docs/` y los Issues.
4. Conforme se agreguen aplicaciones reales, documentar aquí sus comandos de instalación, ejecución, pruebas y configuración. No se anuncian comandos ni dependencias aún no implementados.

## Criterio de terminado (Definition of Done)

Código legible; alcance del Issue cubierto; sin secretos; validaciones correspondientes; prueba manual o automatizada descrita; documentación ajustada; PR revisado e integrado en `develop`. Una entrega en `main` requiere comprobación de integración y despliegue autorizado.

## Licencia

No se concede una licencia de reutilización hasta que el equipo defina y añada una licencia explícita. Repositorio público no significa código de uso libre.
