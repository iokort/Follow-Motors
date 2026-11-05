# Alcance vigente — Follow Motors

**Follow Motors es el portal de asistencia y posventa de una empresa con incorporación selectiva de clientes.** No es un ecommerce ni un sistema de registro público.

| Área | Público | Función |
| --- | --- | --- |
| Información de soporte | Visitantes | Servicios de mantenimiento, información publicada, políticas, contacto y sucursales; sin venta ni inventario |
| Mi Follow Motors | Clientes ya habilitados | Solo motos vinculadas a la cuenta, historial, kilometraje y solicitudes autorizadas |
| Operación de sucursal | Asesor y gerencia autorizados | Verificar y registrar clientes, vincular motos, gestionar citas y servicios, auditar operaciones |

## Reglas
- El cliente **acude a sucursal** y un asesor autorizado verifica su identidad y motocicletas antes del alta. No existe registro autónomo desde el navegador.
- El asesor puede proponer y registrar un expediente según políticas empresariales; no se asume que cualquier persona deba ser admitida.
- Credenciales y activación de cuenta solo siguen el mecanismo que apruebe la empresa.
- Un cliente no puede vincular una moto arbitraria desde internet ni consultar la de otro cliente.
- Si no tiene ninguna moto vinculada, mostrar **«No hay motocicletas vinculadas a tu cuenta. Acude a tu sucursal para verificar el registro.»** No rellenar con catálogo o sugerencias.
- El catálogo público es **informativo de asistencia/servicios**, no una invitación a comprar; el acceso privado es para soporte de unidades propias.
- Se auditan las vinculaciones, correcciones, accesos denegados y decisiones de alta.

## Matriz propuesta de permisos

| Función | Visitante | Cliente | Asesor autorizado | Gerencia |
| --- | --- | --- | --- | --- |
| Consultar ayuda y catálogo informativo | Sí | Sí | Sí | Sí |
| Crear cuenta desde web pública | No | No | No | No |
| Registrar cliente y moto verificados en sucursal | No | No | Sí | Según permisos |
| Consultar motos del titular | No | Solo propias | Expedientes autorizados | Según permisos |
| Añadir moto por autoservicio | No | No | No | No |
| Administrar roles y altas | No | No | No | Rol autorizado |

La matriz queda sujeta a validación con la gerencia; el backend es siempre responsable de imponerla.

## Pruebas de aceptación esenciales
- No se expone ningún formulario ni ruta pública para crear cuenta.
- Solicitud de alta sin sesión o sin permiso devuelve 401/403.
- El asesor autorizado puede efectuar el alta válida con registro de auditoría.
- El cliente A no puede consultar motocicletas de B.
- El cliente sin motocicletas ve estado vacío y no catálogo ajeno.
- La información pública es accesible sin cuenta y no contiene venta/compra.

[ADR-0005](adr/0005-alta-presencial-posventa.md) · [Backlog V5](backlog-v5.md)
