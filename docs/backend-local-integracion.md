# Backend local — operaciones de taller

El sistema utiliza ASP.NET Core 10, MediatR 12, Dapper y MySQL 8. El frontend Angular en `Frontend/` permanece sin cambios. El API original sigue funcionando en las rutas `/api/autenticacion`, `/api/clientes`, `/api/atenciones`, etc.

## Configuración local

1. Instalar .NET SDK 10, Node.js y MySQL 8.
2. Importar `Backend/database/schema_eer.sql` en MySQL local (el esquema conserva su nombre existente).
3. Definir las variables de entorno `ConnectionStrings__MySql`, `Jwt__Clave`, `Jwt__Emisor` y `Jwt__Audiencia`. La contraseña y el secreto JWT nunca se incluyen en Git.
4. Establecer `Demo__Activado=false` en la configuración local para utilizar MySQL; el modo demostración no persiste datos.
5. Ejecutar el proyecto `Backend/Presentation/` y Angular desde `Frontend/`; configurar el proxy para usar el puerto HTTP seleccionado.

## Rutas nuevas

Todas las rutas requieren JWT. Las operaciones de personal requieren GERENTE o EMPLEADO; las resoluciones de cancelación y reasignación de mecánico requieren GERENTE.

| Operación | Verbo | Ruta |
|---|---|---|
| Crear y consultar presupuesto | POST / GET | /api/operaciones/presupuestos |
| Decidir presupuesto | POST | /api/operaciones/presupuestos/decision |
| Contratar y consultar orden | POST / GET | /api/operaciones/ordenes |
| Mecánico, avance, finalizar, entregar | POST | /api/operaciones/ordenes/... |
| Pedir, recibir, usar y devolver piezas | POST | /api/operaciones/refacciones/... |
| Anticipo único | POST | /api/operaciones/anticipos |
| Solicitar y resolver cancelación | POST | /api/operaciones/cancelaciones/... |
| Mantenimiento programado | POST / GET | /api/operaciones/mantenimientos/... |
| Notificaciones y lectura | GET / POST | /api/operaciones/notificaciones/... |
| Garantía por orden | GET | /api/operaciones/garantias/orden/{id} |

## Integridad

Cada operación de varias escrituras se ejecuta con transacción MySQL. Los cambios de estado se filtran por el estado vigente. El progreso se obtiene de los pesos de tareas; los reintegros son obligaciones calculadas y no equivalen a reembolsos efectivamente pagados. Las notificaciones se conservan localmente en MySQL; no envían correos ni dependen de servicios externos.

## Límites de validación

Los cambios deberán compilarse con `dotnet build Backend/FollowMotors.sln` y probarse contra MySQL real antes de fusionar. Ningún resultado de integración, aceptación o despliegue se presume sin ejecutar las pruebas.
