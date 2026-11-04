# Entornos y despliegue

## Preparación local
1. Instalar Docker + Compose y (opcionalmente) .NET SDK 10.
2. Desde la raíz, copiar `.env.example` a `.env` y cambiar ambas contraseñas de ejemplo.
3. Ejecutar `docker compose up -d --build`.
4. Abrir `http://localhost:5099/api/v1/health` y `http://localhost:5099/api/v1/health/database`.
5. Servir `frontend/` por HTTP local en puerto 5500, por ejemplo con Live Server.

MySQL se publica **solo** en `127.0.0.1:3307`, la API en `127.0.0.1:5099`. No se publican credenciales. `docker compose down` detiene servicios sin borrar el volumen de MySQL. Evitar `down -v` salvo intención explícita de eliminar datos.

## Producción
La web de referencia en `iokort.com/follow-motors` **no se ha modificado**. Antes de desplegar:
- Confirmar que el alojamiento destino permite ejecución ASP.NET Core y su versión requerida; si no, separar frontend estático y API en un servidor compatible.
- Configurar HTTPS, proxy/origen público de API, CORS, variables secretas y monitoreo.
- Planificar migraciones de base de datos con respaldo verificado y rollback.
- Comprobar rutas críticas con datos de prueba; revisar roles y autenticación.
- Aprobar PR de `develop` a `main` y publicar de forma controlada.

**No existe despliegue automático**, para evitar sobrescribir la aplicación existente.
