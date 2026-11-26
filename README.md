# FollowMotors · Demostración funcional

Proyecto académico con Angular y ASP.NET Core (.NET 10). Incluye el backend parcial por capas, interfaces, CQRS, MediatR, FluentValidation y JWT, acompañado por una interfaz Angular para los flujos implementados.

## Ejecución en Windows

Requisitos: **SDK de .NET 10**, **Node.js 22.12 o posterior**, npm y conexión a internet para descargar paquetes la primera vez. No requiere MySQL.

1. Descomprimir el ZIP completo en una carpeta local.
2. Ejecutar **`iniciar.bat`** desde la carpeta raíz.
3. El script comprueba .NET y Node/npm, instala dependencias Angular si faltan, abre la API en `http://127.0.0.1:5148` y Angular en `http://localhost:4200`.
4. En la pantalla de acceso, elegir una cuenta ficticia y pulsar «Entrar al sistema».
5. Para lanzar una prueba secuencial de los casos de uso, ejecutar **`probar-flujos.bat`** con la API encendida.

Cerrar la ventana de API detiene el servidor; los registros ficticios se reinician al iniciarlo de nuevo. Los cambios durante una misma ejecución sí se conservan.

## Modo de datos en memoria

Toda la información inicial ficticia y las implementaciones temporales de los repositorios están en:

`Backend/Infrastructure/Demo/DemoDatosEnMemoria.cs`

Los demás archivos C# conservan sus responsabilidades; Angular se conecta a los endpoints reales y no contiene una base de datos simulada propia. La estructura sigue organizada en **Domain**, **Application**, **Infrastructure** y **Presentation**. Cuando se active MySQL posteriormente, las implementaciones existentes están en `Backend/Infrastructure/Repositories`, y `Backend/Infrastructure/DependencyInjectionInfrastructure.cs` mantiene su registro.

`Backend/Presentation/appsettings.Development.json` activa `Demo:Activado`. `Backend/Presentation/Main.cs` utiliza la memoria **solo en el entorno Development**. En un entorno no demo, la API seguirá necesitando `ConnectionStrings:MySql`, configuración de JWT y las variables de inicialización gerencial correspondientes.

### Accesos ficticios

Las cuatro cuentas se muestran automáticamente en Angular al consultar `GET /api/demo/accesos` (disponible exclusivamente en modo demo). Incluyen gerente, asesor, mecánico y cliente. Las contraseñas ficticias residen en el archivo de datos C# y se almacenan en memoria como hashes PBKDF2, no en texto claro durante la autenticación.

**Nunca utilices estas cuentas para producción ni actives los endpoints demo con información real.** El JWT se firma con una clave aleatoria nueva cada vez que se inicia el servidor.

## Flujos que se pueden demostrar

| Acción | Endpoint | Restricción |
| --- | --- | --- |
| Inicio de sesión | `POST /api/autenticacion/iniciar-sesion` | Cuenta activa |
| Perfil del gerente | `GET /api/gerencia/mi-perfil` | GERENTE |
| Crear cliente presencial | `POST /api/clientes` | GERENTE, EMPLEADO |
| Desactivar cliente | `DELETE /api/clientes/{id}` | GERENTE |
| Registrar personal | `POST /api/personal` | GERENTE |
| Listar personal | `GET /api/personal` | GERENTE |
| Registrar motocicleta | `POST /api/motocicletas` | GERENTE, EMPLEADO |
| Ver motocicletas propias | `GET /api/motocicletas/mis-motocicletas` | CLIENTE |
| Ver expediente propio | `GET /api/clientes/mi-expediente` | CLIENTE |
| Recibir motocicleta | `POST /api/atenciones` | GERENTE, EMPLEADO |
| Registrar diagnóstico | `POST /api/atenciones/{id}/diagnostico` | Gerente o mecánico asignado |
| Cobrar revisión | `POST /api/atenciones/{id}/cobro` | GERENTE, EMPLEADO |
| Consultar atención | `GET /api/atenciones/{id}` | Rol o propiedad autorizada |
| Consultar auditoría | `GET /api/auditoria` | GERENTE |

El endpoint temporal `GET /api/demo/catalogos` ayuda a llenar los formularios con IDs reales de la demostración. Se usa el mismo backend para mostrar cambios tras cada operación.

En Angular, al realizar una operación, el panel inferior «Recorrido del caso de uso» muestra los archivos que intervienen. Es una guía de lectura de código, no una traza automática instrumentada.

## Pendiente de esta entrega

**No se implementaron los módulos de presupuestos, anticipos, garantías, reparación, refacciones, citas ni cancelaciones.** Sus archivos originales permanecen como base documental donde existían; no se agregaron flujos ficticios para simular su cumplimiento.

## Comprobación de la entrega

El ZIP incluye el código fuente C# y Angular y los scripts de inicio y pruebas. La estructura del proyecto, las rutas, el JSON y las referencias estáticas se revisaron. **No se afirma compilación ni ejecución real de extremo a extremo sin ejecutar `dotnet` y `npm` en el equipo de destino.** `probar-flujos.bat` está incluido para verificar los endpoints una vez iniciado el sistema.
