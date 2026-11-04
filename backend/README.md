# Backend — Kort

API REST mínima en **.NET 10 LTS** (ASP.NET Core) con MySqlConnector 2.6.2.

## Ejecutar localmente
```bash
dotnet restore backend/FollowMotors.Api/FollowMotors.Api.csproj
dotnet run --project backend/FollowMotors.Api/FollowMotors.Api.csproj
```

La URL de desarrollo (perfil `http`) es http://localhost:5099.

```text
GET /api/v1/health             -> 200; no requiere MySQL
GET /api/v1/health/database    -> 200/503; solo en Development, requiere conexión configurada
```

## MySQL local
Desde la raíz, copiar `.env.example` a `.env`, cambiar contraseñas y ejecutar:

```bash
docker compose up -d --build
```

Docker expone MySQL solo en `127.0.0.1:3307` y la API en `127.0.0.1:5099`. Para ejecutar la API fuera de Docker, establecer `ConnectionStrings__Default` en el entorno (servidor `localhost`, puerto `3307`).

La comprobación de base de datos solo prueba conectividad; **todavía no existe esquema ni lógica de negocio**. Gael es responsable del modelo y las migraciones.

## Diseño
El código HTTP está separado de la creación de conexiones a través de `IDatabaseConnectionFactory`. Crear interfaces y servicios adicionales únicamente cuando existan casos de uso, manteniendo el backend como único consumidor de MySQL.

No usar el alojamiento PHP de iokort.com para ejecutar .NET sin validar primero infraestructura compatible.
