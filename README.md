# FollowMotors

Sistema académico de gestión y seguimiento de servicios para un **taller de motocicletas multimarca**. FollowMotors **no vende motocicletas** y **no gestiona garantías del fabricante**.

## Aplicaciones y datos

| Componente | Tecnología | Usuarios | Propósito |
| --- | --- | --- | --- |
| Aplicación administrativa | C# / .NET (escritorio) | Gerente y empleados operativos | Altas presenciales, citas, diagnósticos, presupuestos, servicios y seguimiento |
| API de negocio | ASP.NET Core Web API (C#) | Consumida por ambas aplicaciones | Autorización, reglas de negocio, CQRS y acceso a MySQL |
| Portal de clientes | Angular + TypeScript | Clientes con cuenta | Consultas privadas, reserva automática de citas y solicitud de cancelación |
| Persistencia | MySQL | Solo API | Expedientes y trazabilidad |
| Colaboración | Git y GitHub | Equipo | Issues, ramas, pull requests y pruebas |

**Ejecución prevista:** académica y local. La base MySQL ya existe en Hostinger pero su esquema se definirá a partir de las historias. No se requiere desplegar la API en producción ni contratar un VPS. El subdominio `followmtrs.iokort.com` está reservado, no es requisito de entrega.

**Importante:** el programa de escritorio y Angular no se conectan directamente a MySQL. La API C# gestiona los permisos y operaciones. `@follow.motors` es un **identificador ficticio de cuenta**, no un buzón real.

## Documentación funcional

Comenzar por el [índice de readmis](readmis/README.md), que enlaza:

1. [Concepto, actores y alcance](readmis/00-concepto.md).
2. [Reglas de negocio](readmis/01-reglas-negocio.md).
3. [Flujos funcionales](readmis/02-flujos.md).
4. [Arquitectura y flujo de GitHub](readmis/03-arquitectura-github.md).
5. [Trazabilidad y pruebas](readmis/04-trazabilidad-pruebas.md).

## Épicas e historias

| Épica | Alcance | Historias |
| --- | --- | --- |
| [E01](readmis/epicas/E01-identidad-expedientes/README.md) | Identidad y expedientes | US01–US03 |
| [E02](readmis/epicas/E02-citas-recepcion/README.md) | Agenda y recepción | US04–US06 |
| [E03](readmis/epicas/E03-contratacion-recursos/README.md) | Presupuesto, cobro y recursos | US07–US09 |
| [E04](readmis/epicas/E04-ejecucion-cierre/README.md) | Ejecución, cancelación y cierre | US10–US12 |

Cada historia contiene propósito, criterios de aceptación, flujo y **entre 5 y 10 tarjetas CRC**. Las tarjetas son candidatas de análisis: **una tarjeta no obliga a crear una clase nueva**, y una misma clase puede participar en varias historias.

## Estado del trabajo

Este reinicio documenta **análisis y diseño funcional**. No implica código implementado, pruebas ejecutadas ni despliegue. El historial de ramas, PR e Issues anteriores es antecedente; no acredita que estas 12 historias estén terminadas.

El proyecto se entrega al finalizar el cuatrimestre; no se planifica una fase posterior de producción.
