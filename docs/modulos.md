# Inventario funcional de referencia

Vistas observadas en el sitio actual de Follow Motors (Hostinger):

| Área | Pantallas |
| --- | --- |
| Acceso | Login, registro |
| Cliente | Panel de cliente, actualizar kilometraje |
| Gerente | Panel de gerente, cuentas de clientes, registrar unidad, agenda de servicios |
| Seguimiento | Primer servicio, segundo servicio, tercer servicio, garantías finalizadas |

## Trabajo previo a implementación

Aarón documenta propósito, actores, entradas/salidas, reglas y criterios de aceptación de cada flujo.
Vixtor Fernando define interfaz y campos necesarios.
Gael propone entidades y relaciones.
Kort consolida el contrato de API y conecta frontend con backend.

Este inventario describe nombres de archivos encontrados en el sitio anterior; **no** certifica que todas las funciones estén completas o sean correctas.

## Cambio de producto

La antigua pantalla de registro se conserva únicamente como referencia histórica. La nueva versión no tendrá autorregistro: el asesor autorizado de sucursal verifica, crea y vincula clientes y motos. El catálogo es informativo de asistencia, y el portal privado muestra solo unidades del titular. [Alcance](alcance-posventa.md).
