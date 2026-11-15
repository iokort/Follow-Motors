# 03 — Arquitectura, SOLID, CQRS y colaboración

## Decisión técnica para esta entrega académica

- Administrador: aplicación de escritorio **C# / .NET** (gerente y empleado).
- Cliente: portal **Angular + TypeScript**.
- Negocio: **API ASP.NET Core en C#**, ejecutada **localmente** para desarrollo y demostración.
- Datos: **MySQL** ya creada en Hostinger; conexión del backend mediante configuración segura y permisos de red autorizados.
- Sin PHP, Node.js como backend alternativo, ni VPS obligatorio.
- Los clientes del sistema **no conocen credenciales de MySQL**. Solo la API se conecta a la BD.

## Capas y carpetas (organizar primero por responsabilidad)

```text
FollowMotors/
├── desktop/                # Cliente administrativo C# (UI por decidir)
├── frontend/               # Angular para propietarios
├── backend/
│   ├── FollowMotors.Api/
│   │   └── Controllers/    # Solo HTTP, delegan; sin lógica de negocio
│   ├── FollowMotors.Application/
│   │   ├── Commands/       # Escribir/cambiar estado
│   │   ├── Queries/        # Consultar
│   │   ├── Handlers/       # Orquestar casos de uso
│   │   ├── Validators/     # Validación de solicitudes
│   │   ├── Interfaces/     # Contratos por necesidad real
│   │   └── DTOs/
│   ├── FollowMotors.Domain/
│   │   ├── Entities/
│   │   ├── Rules/
│   │   └── Events/
│   └── FollowMotors.Infrastructure/
│       ├── Persistence/
│       └── Repositories/
└── tests/
    ├── Unit/
    └── Integration/
```

Dentro de cada directorio, agrupar por **subdominio** cuando haga falta (`Clientes`, `Citas`, `Servicios`). Evitar la estructura `UnaHistoria/Controller+Handler+Validator+Interfaz`.

**SOLID** se refleja en responsabilidades y dependencias, no en el número de carpetas o interfaces. **CQRS** separa escrituras (`Commands`) de lecturas (`Queries`) en la misma API y la misma base MySQL; no obliga a duplicar bases. **MediatR** (o equivalente indicado por la materia) despacha peticiones a handlers. **FluentValidation** valida los comandos y consultas que lo necesiten. Los **controllers** solo transforman HTTP, delegan y devuelven respuestas.

**MVVM/MVP:** son conceptos de presentación, no requisitos para reescribir el backend. Si la materia exige MVVM, puede aplicarse a la interfaz de escritorio si se elige una tecnología compatible, y en Angular se mantiene separación de componentes, estado de presentación y servicios; no es necesario implementar dos patrones alternativos a la fuerza. `MVP` también puede referirse al alcance mínimo viable, que no es lo mismo que Model–View–Presenter.

## Reglas de seguridad mínimas

- API autoriza por rol y por propiedad del recurso: un cliente no obtiene motos ajenas aunque manipule IDs.
- No exponer conexión MySQL ni credenciales en Angular, ejecutables, `appsettings.json` versionado o documentación.
- Una cuenta `@follow.motors` **no es un correo entregable**. No diseñar recuperación vía ese dominio.
- Contraseñas con hash seguro, autenticación/sesiones, desactivación de cuentas, validación de entrada y auditoría básica.
- Dos acciones concurrentes sobre cita/avance/cancelación deben respetar invariantes mediante transacciones.

## GitHub Flow para el equipo

1. Elegir **una historia** y documentar sus criterios en una Issue nueva; no dar por vigentes Issues antiguos.
2. Crear rama desde `main`: `feature/us04-reservas`, `feature/us11-cancelaciones`, etc.
3. Commits pequeños y descriptivos; no subir secretos ni dumps de datos reales.
4. Abrir Pull Request con referencia a la historia y evidencia de pruebas.
5. Revisar código y CI, integrar a `main` cuando cumpla criterios. No mezclar módulos ajenos.
6. Para correcciones posteriores, crear otra rama y PR.

Las ramas y PR anteriores pueden conservarse como archivo histórico. **No fusionarlos al nuevo análisis sin revisión.** Si las protecciones o checks antiguos bloquean documentación, resolver esa política conscientemente; no usar force-push.

## Pruebas

- **Unitarias:** cálculo de avance y dinero, límite 60 %, pesos de tareas, selección de mecánico, autorización de roles, validaciones.
- **Integración:** reservas concurrentes, autorización por propietario, escritura/lectura MySQL, anticipo y cancelación transaccional.
- **Manuales E2E:** escritorio → API → MySQL → Angular, desde alta hasta entrega.
- Base de pruebas aislada; no ejecutar suites destructivas contra la base compartida de Hostinger.

**Estado:** este documento es propuesta arquitectónica; no acredita implementación ni ejecución de tests.
