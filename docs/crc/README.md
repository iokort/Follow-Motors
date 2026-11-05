# CRC extendidas por historias de usuario — Diseño propuesto

El [catálogo inicial de 15 CRC](../tarjetas-crc.md) procedía de la planificación y el catálogo original. La ampliación organiza **20 fichas contextuales por cada historia US01–US08**, más **US21** como control transversal.

**9 historias × 20 asociaciones = 180 fichas**, construidas sobre **105 componentes/conceptos reutilizables**. Las fichas repetidas de una misma clase comparten definición; **no son 180 clases diferentes ni 180 archivos C# implementados**.

| Historia | Área | Fichas CRC |
| --- | --- | ---: |
| [US01](./US01.md) | Inicio de sesión del gerente | 20 |
| [US02](./US02.md) | Inicio de sesión del cliente | 20 |
| [US03](./US03.md) | Cierre de sesión seguro | 20 |
| [US04](./US04.md) | Alta de cliente y motocicleta | 20 |
| [US05](./US05.md) | Administrar clientes y unidades | 20 |
| [US06](./US06.md) | Agenda de citas del taller | 20 |
| [US07](./US07.md) | Seguimiento de mantenimiento | 20 |
| [US08](./US08.md) | Portal privado del cliente | 20 |
| [US21](./US21.md) | Políticas de roles y permisos (transversal) | 20 |

## Lectura recomendada

1. Abrir la historia del trabajo, por ejemplo [US04](US04.md).
2. Consultar 20 fichas con tipo, **responsabilidad específica**, colaboradores, regla SOLID y límite de diseño.
3. Buscar las interfaces a implementar y descartar las abstracciones que no aporten valor.
4. Relacionar el componente finalmente implementado con la historia, PR y prueba correspondiente.
5. Mantener las mismas clases compartidas entre historias, sin crear copias por US.

## Advertencia de diseño

Las 20 fichas por historia son una **descomposición académica de análisis**, no un mínimo exigido por SOLID. En aplicación real algunos conceptos se implementan como funciones, DTO, políticas, middleware, interfaces o clases; otros pueden consolidarse con justificación de cohesión. El criterio es responsabilidad verificable, no cantidad.

**Fuente del alcance:** documento de planificación integral V5 (30/10/2026), catálogo CRC original de 15 tarjetas y requerimientos US01–US08/US21.
