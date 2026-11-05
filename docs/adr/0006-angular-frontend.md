# ADR-0006 — Angular como frontend definitivo

**Estado:** Decisión tecnológica confirmada por el proyecto.  
**Fecha:** 4 de noviembre de 2026.

## Stack decidido

- Frontend: **Angular 22**, TypeScript, componentes standalone, signals, plantillas y CSS.
- Backend: **C# / ASP.NET Core .NET 10**, API REST en `/api/v1`.
- Base de datos: **MySQL 8.4**, utilizada solo por la API C#.

## Motivos y aplicación

Se sustituye el prototipo inicial sin framework por Angular, con componentes tipados y un cliente HTTP inyectable. El esquema de presentación se inspira en MVVM: vista en templates, estado/reactividad mediante signals y modelos como DTO. No se crean clases ViewModel redundantes por obligación. SOLID se aplica de forma proporcional.

## Efectos de la decisión

- Requiere Node.js 22.22.3+, 24.15+ o 26+ y TypeScript 6.0.x.
- CI compila Angular además del backend .NET/MySQL.
- El frontend se empaqueta como archivos estáticos; su hosting y el de la API pueden ser diferentes.
- La autorización, propiedad de motocicletas y alta de sucursal siempre se validan en C#; Angular no accede directamente a MySQL.
- Los módulos funcionales están pendientes: el nuevo frontend inicial solamente comprueba salud de la API.

## Fuentes

Angular. (2026). *Version compatibility*. https://angular.dev/reference/versions

Angular. (s. f.). *Building Angular apps*. https://angular.dev/tools/cli/build
