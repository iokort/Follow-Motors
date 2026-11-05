# Arquitectura, MVVM y SOLID

[Inicio](Home) · [Historias y CRC](Historias-y-CRC) · [Alcance](Alcance-y-Sucursal)

## Tecnologías
Frontend **Angular 22 con TypeScript**, API **ASP.NET Core .NET 10 (C#)** y persistencia **MySQL 8.4**. Se prefiere monolito modular, no microservicios prematuros.

## MVVM ligero
- **View**: templates de componentes Angular HTML/CSS y accesibilidad.
- **ViewModel conceptual**: signals y estado en componentes o servicios de presentación Angular.
- **Model**: interfaces/DTO TypeScript y servicios HttpClient; el dominio de negocio y los permisos se aplican en el servidor C#.
- El **backend** valida identidad, rol y relación cliente–motocicleta. La vista jamás autoriza.

## SOLID
Responsabilidad única; extensibilidad justificada; contratos sustituibles; interfaces limitadas y dependencias invertidas cuando aporten testabilidad. No crear clases de relleno por cumplir una cantidad.

## Recursos
[Diagrama de clases](https://github.com/iokort/Follow-Motors/blob/main/docs/diagramas/clases.md) · [Secuencias](https://github.com/iokort/Follow-Motors/blob/main/docs/diagramas/secuencias.md) · [MVVM y arquitectura](https://github.com/iokort/Follow-Motors/blob/main/docs/mvvm-y-arquitectura.md)

**Estado:** arquitectura objetivo; autenticación, permisos y operaciones de negocio aún no están completas.

---

© 2026 Kurt Cobain Vázquez Sánchez · [ioKort](https://iokort.com)
