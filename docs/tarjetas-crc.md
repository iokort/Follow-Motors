# Tarjetas CRC — Follow Motors

**Ampliación por historias:** [180 fichas contextuales, 20 para cada US01–US08 y US21](crc/README.md). Este documento conserva las 15 CRC originales como línea base compartida.

**CRC** significa *Class, Responsibility, Collaborator*. Cada tarjeta representa responsabilidades y relaciones entre componentes; **no garantiza que ya exista la clase en C#**. Estas 15 tarjetas son propuestas de diseño derivadas del expediente V5 y deben revisarse conforme se implementen los módulos.

## Conjunto de tarjetas

### CRC-01 · Usuario

| Clase / componente | Responsabilidades | Colabora con |
| --- | --- | --- |
| **Usuario** | Representar identidad, correo y estado; jamás almacenar contraseñas en claro | Sesion, Cliente |

**Estado:** Diseño propuesto, pendiente de validación en código y pruebas.


### CRC-02 · Gerente (rol)

| Clase / componente | Responsabilidades | Colabora con |
| --- | --- | --- |
| **Gerente (rol)** | Solicitar operaciones gerenciales según políticas de autorización; evitar atributos administrativos duplicados | Usuario, GestorClientes, GestorCitas |

**Estado:** Diseño propuesto, pendiente de validación en código y pruebas.


### CRC-03 · Cliente

| Clase / componente | Responsabilidades | Colabora con |
| --- | --- | --- |
| **Cliente** | Mantener datos de contacto y relación con unidades propias | Usuario, Motocicleta |

**Estado:** Diseño propuesto, pendiente de validación en código y pruebas.


### CRC-04 · Sesion

| Clase / componente | Responsabilidades | Colabora con |
| --- | --- | --- |
| **Sesion** | Representar vigencia e invalidación de identidad autenticada | Usuario, AutenticacionServicio |

**Estado:** Diseño propuesto, pendiente de validación en código y pruebas.


### CRC-05 · AutenticacionServicio

| Clase / componente | Responsabilidades | Colabora con |
| --- | --- | --- |
| **AutenticacionServicio** | Verificar credenciales, iniciar/cerrar sesión y aplicar protección de acceso | Usuario, Sesion |

**Estado:** Diseño propuesto, pendiente de validación en código y pruebas.


### CRC-06 · Motocicleta

| Clase / componente | Responsabilidades | Colabora con |
| --- | --- | --- |
| **Motocicleta** | Conservar VIN, modelo, propietario y kilometraje actual bajo reglas de negocio | Cliente, PlanMantenimiento |

**Estado:** Diseño propuesto, pendiente de validación en código y pruebas.


### CRC-07 · GestorClientes

| Clase / componente | Responsabilidades | Colabora con |
| --- | --- | --- |
| **GestorClientes** | Crear, actualizar, consultar y detectar duplicación de clientes | Cliente, GestorMotocicletas |

**Estado:** Diseño propuesto, pendiente de validación en código y pruebas.


### CRC-08 · GestorMotocicletas

| Clase / componente | Responsabilidades | Colabora con |
| --- | --- | --- |
| **GestorMotocicletas** | Registrar, asociar, consultar y validar propiedad/permiso de unidades | Motocicleta, Cliente |

**Estado:** Diseño propuesto, pendiente de validación en código y pruebas.


### CRC-09 · CitaServicio

| Clase / componente | Responsabilidades | Colabora con |
| --- | --- | --- |
| **CitaServicio** | Representar horario, motivo, unidad y estado de la atención | Motocicleta, Cliente |

**Estado:** Diseño propuesto, pendiente de validación en código y pruebas.


### CRC-10 · GestorCitas

| Clase / componente | Responsabilidades | Colabora con |
| --- | --- | --- |
| **GestorCitas** | Verificar disponibilidad y gestionar reservas según capacidad y rol | CitaServicio, GestorMotocicletas |

**Estado:** Diseño propuesto, pendiente de validación en código y pruebas.


### CRC-11 · PlanMantenimiento

| Clase / componente | Responsabilidades | Colabora con |
| --- | --- | --- |
| **PlanMantenimiento** | Calcular próximos servicios a partir de reglas versionadas y aceptadas | Motocicleta, RegistroServicio |

**Estado:** Diseño propuesto, pendiente de validación en código y pruebas.


### CRC-12 · RegistroServicio

| Clase / componente | Responsabilidades | Colabora con |
| --- | --- | --- |
| **RegistroServicio** | Conservar evidencia histórica de etapa, fecha, kilometraje y autor | Motocicleta, PlanMantenimiento |

**Estado:** Diseño propuesto, pendiente de validación en código y pruebas.


### CRC-13 · GestorMantenimiento

| Clase / componente | Responsabilidades | Colabora con |
| --- | --- | --- |
| **GestorMantenimiento** | Aplicar transiciones de servicio y persistir historial de forma consistente | Motocicleta, RegistroServicio |

**Estado:** Diseño propuesto, pendiente de validación en código y pruebas.


### CRC-14 · SolicitudServicio

| Clase / componente | Responsabilidades | Colabora con |
| --- | --- | --- |
| **SolicitudServicio** | Representar motivo, unidad, solicitante y estado de solicitud | Cliente, Motocicleta |

**Estado:** Diseño propuesto, pendiente de validación en código y pruebas.


### CRC-15 · GestorSolicitudes

| Clase / componente | Responsabilidades | Colabora con |
| --- | --- | --- |
| **GestorSolicitudes** | Validar, registrar, consultar y resolver solicitudes de atención | SolicitudServicio, Cliente |

**Estado:** Diseño propuesto, pendiente de validación en código y pruebas.



## Criterios para mantener las tarjetas

- Cada responsabilidad debe poder vincularse a un caso de uso o regla de negocio real.
- Evitar clases con responsabilidades de UI, datos y autenticación mezcladas.
- Los roles son políticas/composición, no obligatoriamente subclases de `Usuario`.
- `PlanMantenimiento` no fija intervalos concretos hasta que gerencia los apruebe.
- Revisar colaboraciones al diseñar API, esquema y pruebas.

## Referencia

Beck, K., & Cunningham, W. (1989). A laboratory for teaching object-oriented thinking. *ACM SIGPLAN Notices, 24*(10), 1–6. https://doi.org/10.1145/74878.74879
