# 00 — Concepto general y alcance

## Propósito

FollowMotors es un **taller de servicios y reparaciones para motocicletas de cualquier marca y procedencia**. El sistema permite organizar el trabajo administrativo y técnico, y dar al propietario seguimiento privado a sus motocicletas y servicios.

No es un sitio de venta de motocicletas. La cuenta del cliente puede crearse sin comprar, contratar ni registrar motocicleta alguna; el registro se hace **en persona y sin cita**.

## Actores

| Actor | Aplicación | Alcance |
| --- | --- | --- |
| Gerente | Escritorio C# | Opera el taller, desactiva cuentas, administra empleados y resuelve excepciones autorizadas |
| Empleado operativo | Escritorio C# | Registra clientes y motos, atiende agenda y recepción, administra operaciones habituales |
| Mecánico | Función operativa del personal, sin tercer nivel de permiso obligatorio | Diagnostica, estima mano de obra y entrega, define tareas, registra ejecución y refacciones |
| Cliente | Portal Angular | Consulta solo sus datos; reserva citas y solicita cancelaciones sujetas a regla |
| Proveedor | Fuera del sistema | Entrega refacciones solicitadas; no tiene cuenta ni acceso al software |

Los **dos niveles internos de acceso** son gerente y empleado. El mecánico tiene responsabilidades funcionales distinguibles, pero no obliga a crear un tercer rol de autorización.

## Productos

1. **Aplicación de escritorio C#:** herramienta de gerente y personal; realiza operaciones administrativas.
2. **Portal Angular:** consulta privada de datos de motocicleta, historial, mantenimiento, garantía, citas y avance del taller; además, permite reservar citas y solicitar cancelaciones.
3. **API ASP.NET Core:** backend C# compartido por las dos aplicaciones. Es el único componente con conexión a la base de datos MySQL.
4. **MySQL:** expedientes, agenda, órdenes, tareas, pagos, garantías, solicitudes y auditoría.

## Ciclo de negocio

Persona → registro presencial opcional → cita presencial o reserva web → ingreso de motocicleta → revisión con tarifa fija (pagada ese día) → diagnóstico y mano de obra acordada → pago anticipado de reparación y entrega de condiciones de garantía → solicitud y uso de refacciones → tareas de reparación → entrega e historial.

Si el cliente no acepta reparar, **se conserva el diagnóstico y se cobra la revisión**. Una reparación contratada también admite cancelación dentro de las reglas de progreso.

## No incluido

Venta, financiamiento y catálogo comercial de motos; garantías de fábrica; proveedor como usuario; almacén/inventario completo; facturación fiscal electrónica automática; correos reales `@follow.motors`; despliegue productivo ni VPS.

## Datos principales del negocio (candidatos, no tablas definitivas)

Cliente, Cuenta, Motocicleta, Mecánico/Empleado, Disponibilidad, Cita, Atención/Diagnóstico, Presupuesto, Orden de servicio, Tarea, Refacción requerida/utilizada, Pago, Garantía del taller, Solicitud de cancelación y Comprobante.

**Decisiones operativas adoptadas para completar el análisis:** citas con cupos configurables de revisión (sin depender de que un mecánico esté totalmente libre); alta de cuenta antes o después del primer servicio; desglose de anticipo por mano de obra y piezas; duración estimada comunicada pero no usada para calcular avance; comprobante de garantía entregado al pagar, con vigencia desde la finalización/entrega del trabajo.
