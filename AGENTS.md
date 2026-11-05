# Instrucciones para asistentes y agentes de programación

Follow Motors se reconstruye por cuatro responsables, sin reemplazar la versión productiva.

1. Inspeccionar archivos existentes antes de editar; intervenir solo lo indispensable.
2. Los cargos funcionales son: Aarón=Project Manager (coordinación, tablero, documentación), Gael=Database Manager (MySQL y referente de producto), Vixtor Fernando=Frontend Designer & Developer (Angular), Kort=Backend Developer (C# e integración). No confundir dirección de proceso con autoría del código.
3. Stack definitivo: frontend Angular 22/TypeScript, backend ASP.NET Core .NET 10/C#, datos MySQL 8.4. El navegador no se conecta a MySQL. Evitar prototipos de HTML/JS alternativos.
4. Respetar convenciones de `CONTRIBUTING.md`, arquitectura y contratos de `docs/`.
5. Aplicar SOLID con criterio: simplicidad y comprobabilidad por encima de capas artificiales.
6. Antes de inventar una tabla, endpoint o regla de negocio, documentar propuesta para aprobación del equipo.
7. Nunca subir secretos, datasets personales ni modificar el sitio productivo.
8. Validar lo realmente ejecutado; no afirmar compilación o pruebas que no hayan ocurrido.
9. Preparar PR a `develop`; una entrega a `main` solo después de pruebas integradas y aprobación.
