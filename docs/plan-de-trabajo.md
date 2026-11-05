# Plan de trabajo — Follow Motors

**Alcance vigente:** asistencia y posventa, sin alta pública ni ventas; asesores autorizados crean expedientes en sucursal. [ADR-0005](adr/0005-alta-presencial-posventa.md).

Este plan es **secuencial por dependencias**, no tiene fechas inventadas. Se actualiza mediante Issues y PR. La versión anterior en iokort.com/follow-motors es referencia de procesos, no fuente única de requisitos.

## Fase 0 · Gobierno y entorno

**Resultado:** acceso de colaboradores, responsabilidades, Git CMD/gh, ramas, CODEOWNERS, controles de PR, README, pipeline CI, API técnica, MySQL local y frontend base.

- Kort: verificar CI, permisos, hospedaje compatible con ASP.NET Core, ruta de publicación futura.
- Gael: verificar conexión local a MySQL y entorno de migraciones.
- Fernando: verificar ejecución local del frontend y consumo `/api/v1/health`.
- Aarón: mantener roles, estructura documental y lista de criterios funcionales.
- **Salida:** todos pueden clonar, ejecutar su área y abrir un PR comprobable.

## Fase 1 · Análisis y contratos (trabajo paralelo)

- **Aarón:** fichas de actores cliente/gerente, casos de uso, reglas y criterios verificables para acceso, cuentas, unidades, kilometraje, agenda, servicios y garantías.
- **Fernando:** inventario de pantallas, formularios, navegación y estados de error/carga/vacío.
- **Gael:** diagrama entidad-relación, reglas de integridad, claves, restricciones e índices basados en los requisitos.
- **Kort:** contratos de API (URL, método, DTO, permisos, errores) y estrategia de autenticación; acordarlos con Gael y Fernando.
- **Salida:** primera funcionalidad vertical priorizada, contratos mínimos aprobados y tareas individuales sin bloqueos.

## Fase 2 · Primer flujo extremo a extremo

Empezar por un caso de uso acordado entre todos, **no** desarrollar todas las pantallas sin backend ni toda la BD sin consumidores.

- Aarón detalla criterios y pruebas de aceptación.
- Gael implementa migración versionada y datos ficticios del flujo.
- Kort implementa API, validaciones, permisos y comprobaciones.
- Fernando implementa pantalla y conexión a API, con estados de error y éxito.
- **Salida:** un flujo real que pasa frontend → API → MySQL con pruebas y manual mínimo.

## Fase 3 · Módulos de negocio

Organizar Issues independientes para:
1. Acceso/registro, sesiones y permisos según decisión de autenticación.
2. Clientes y cuentas.
3. Registro y consulta de unidades.
4. Registro y consulta de kilometraje.
5. Agenda de servicios y seguimiento de primer/segundo/tercer servicio.
6. Garantías y sus estados.

Para cada módulo, repetir el ciclo vertical de fase 2. No inventar reglas como intervalos de mantenimiento o garantías sin definición validada.

## Fase 4 · Aseguramiento, despliegue y entrega

- Pruebas de contratos, flujos, permisos, errores y regresiones.
- Validar datos, restricciones y migraciones MySQL; respaldos y restauración.
- Seguridad: HTTPS, secretos, autenticación, autorización, CORS y logs sin datos sensibles.
- Definir servidor compatible con C# .NET; frontend y API pueden alojarse por separado.
- PR `develop` → `main`, aprobación, publicación manual y rollback.
- Manual de usuario, manual técnico, modelo ER y especificación de API actualizados.
- **Salida:** versión aprobada y publicada sin afectar datos o sitio existente inesperadamente.

## Cadencia sencilla

- Una revisión corta de avances y bloqueos al inicio de cada ciclo de integración.
- Cada Issue se mueve conceptualmente por: **Por hacer → En progreso → En revisión → Validado → Terminado**.
- No iniciar una función dependiente de un contrato que todavía no está definido.
- Criterio de terminado: código/propuesta listo, evidencia de prueba, documentación actualizada, CI aprobada, revisión cruzada y verificación del requisito.

## Riesgos abiertos

- Alojamiento de ASP.NET Core en producción aún no confirmado.
- No existe todavía un esquema de base de datos de negocio aprobado.
- Autenticación, permisos y reglas de mantenimiento/garantías están pendientes de especificar.
- No hay despliegue automático por seguridad de la versión actual.
