# Arquitectura inicial

```text
Navegador (HTML/CSS/JS)
        |
        | HTTPS + JSON
        v
API REST (ASP.NET Core / C#)
        |
        | acceso parametrizado y controlado
        v
MySQL
```

## Responsabilidades

- Frontend: interfaz, formularios, validación de experiencia, presentación; sin claves de BD ni SQL en cliente.
- API: autenticación/autorización, validación del lado servidor, casos de uso, reglas de negocio, manejo de errores y acceso a datos.
- Base de datos: integridad referencial, índices, modelo versionado y datos semilla ficticios.
- Documentación: requisitos, trazabilidad, casos de uso, rutas/endpoints y manuales.

## Aplicación de SOLID

- SRP: separar controladores HTTP, lógica de negocio y persistencia.
- DIP: introducir interfaces entre lógica y acceso a datos cuando faciliten pruebas y cambios reales.
- OCP, LSP, ISP: aplicar cuando exista una necesidad concreta; no crear capas o interfaces decorativas.

## Decisiones pendientes

- Definir requisitos verificables de cada módulo heredado.
- Pactar estructura real de entidades/tablas y contratos de API.
- Elegir versión de .NET soportada y proveedor MySQL tras verificar entorno de despliegue.
- Acordar mecanismo de autenticación y roles.
- Determinar si el hosting actual admite el backend C#; no asumir que Hostinger PHP ejecuta ASP.NET Core.

No se debe conectar la aplicación nueva al entorno de producción durante el desarrollo.
