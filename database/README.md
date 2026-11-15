# Base de datos FollowMotors

**Modelo académico v1.0 — MySQL 8.0.16+**, derivado de las 12 historias y cuatro épicas.

## Archivos

- **schema_eer.sql**: crea el esquema lógico y sus 20 tablas, apto para ingeniería inversa en MySQL Workbench.
- **schema_hostinger.sql**: las mismas tablas sin `CREATE DATABASE`, para la base vacía ya existente en Hostinger.
- **diagrama_relacional.mmd**: relaciones visibles también en Mermaid.

## Obtener EER sin conectarse a MySQL

En MySQL Workbench: **File → Import → Reverse Engineer MySQL Create Script**. Selecciona `schema_eer.sql`, marca **Place imported objects on a diagram**, ejecuta y guarda el modelo como `FollowMotors.mwb`. Podrás reorganizar las tablas y sus relaciones.

## Instalar tablas en Hostinger (solo cuando corresponda)

Selecciona `u899160275_follow_motors` en phpMyAdmin e importa **schema_hostinger.sql**. No hay instrucciones DROP, contraseñas ni datos de ejemplo. El script crea tablas en base vacía; **no sirve como migración** sobre un esquema previamente creado.

## Reglas transversales que controla la API C#

Las FKs, CHECK e índices protegen referencias, importes y cantidades. C# debe aplicar las transacciones y autorizaciones que requieren consultar varias filas: 100 % como suma de pesos de tareas, reserva sin exceder capacidad, cliente dueño de la moto, asignación del mecánico menos cargado, venta de piezas sin margen, precio congelado, aviso al 25 %, cancelación hasta 60 % inclusive, reembolso del trabajo pendiente y piezas sin usar, cierre obligatorio si supera 60 %, y entrega de garantía según el servicio.

No hay inicio de sesión a través del dominio ficticio `@follow.motors`: es un identificador, no un buzón. La API es la única capa que se conecta a MySQL y almacena hashes seguros de contraseña.

**Estado:** verificación estática de 20 tablas y 32 FKs. El SQL aún no se ha ejecutado sobre MySQL; no se ha alterado Hostinger.
