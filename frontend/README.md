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

El frontend adopta **MVVM**: los templates y componentes son la View; los ViewModels inyectables mantienen signals y acciones; los modelos TypeScript y servicios `HttpClient` proporcionan datos de la API. La base implementa `src/app/viewmodels/health.viewmodel.ts` como ejemplo real. Se mantienen solo las abstracciones que aporten una responsabilidad clara.

**Alcance:** portal de asistencia y posventa, sin registro público de clientes, motos ni venta. Solo un asesor de sucursal puede dar de alta y vincular unidades. No se conectará MySQL desde Angular.

**Estado:** solo base inicial Angular y comprobación de salud de API. El portal privado, autenticación y operaciones de negocio siguen pendientes.
