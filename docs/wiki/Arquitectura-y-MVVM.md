# Arquitectura, MVVM y SOLID

[Inicio](Home) · [Historias y CRC](Historias-y-CRC) · [Alcance](Alcance-y-Sucursal)

## Tecnologías
Frontend **HTML/CSS/JavaScript**, API **ASP.NET Core C#**, persistencia **MySQL**. Se prefiere monolito modular, no microservicios prematuros.

## MVVM ligero
- **View**: HTML/CSS y accesibilidad.
- **ViewModel**: estado, carga, errores y comandos de interacción JS.
- **Model**: información intercambiada con la API mediante un cliente HTTP desacoplado.
- El **backend** valida identidad, rol y relación cliente–motocicleta. La vista jamás autoriza.

## SOLID
Responsabilidad única; extensibilidad justificada; contratos sustituibles; interfaces limitadas y dependencias invertidas cuando aporten testabilidad. No crear clases de relleno por cumplir una cantidad.

## Recursos
[Diagrama de clases](https://github.com/iokort/Follow-Motors/blob/main/docs/diagramas/clases.md) · [Secuencias](https://github.com/iokort/Follow-Motors/blob/main/docs/diagramas/secuencias.md) · [MVVM y arquitectura](https://github.com/iokort/Follow-Motors/blob/main/docs/mvvm-y-arquitectura.md)

**Estado:** arquitectura objetivo; autenticación, permisos y operaciones de negocio aún no están completas.

---

© 2026 Kurt Cobain Vázquez Sánchez · [ioKort](https://iokort.com)
