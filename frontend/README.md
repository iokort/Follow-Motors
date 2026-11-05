# Frontend Angular — Vixtor Fernando

Stack definitivo: **Angular 22 + TypeScript**, backend **C# / ASP.NET Core .NET 10**, base **MySQL 8.4**.

## Requisitos

Node.js 22.22.3+, 24.15+ o 26+. Consultar https://angular.dev/reference/versions.

## Git CMD

Desde `frontend/`:

```cmd
npm install
npm start
```

Abre http://localhost:4200. El servidor de desarrollo reenvía `/api` a http://localhost:5099 mediante `proxy.conf.json`; ejecuta también el backend.

Para compilar y probar:

```cmd
npm run build
npm test
```

La compilación produce `dist/follow-motors/browser`. El frontend no se publica en producción automáticamente.

## Arquitectura

Los componentes standalone y sus templates presentan la UI; los signals mantienen estado y los servicios con `HttpClient` consultan la API. Se aplica separación inspirada en MVVM sin capas innecesarias.

**Alcance:** portal de asistencia y posventa, sin registro público de clientes, motos ni venta. Solo un asesor de sucursal puede dar de alta y vincular unidades. No se conectará MySQL desde Angular.

**Estado:** solo base inicial Angular y comprobación de salud de API. El portal privado, autenticación y operaciones de negocio siguen pendientes.
