# ADR-0005 — Alta presencial y portal exclusivo de posventa

**Estado:** Decisión de producto comunicada por el responsable del proyecto; pendientes de validación los detalles operativos.  
**Fecha:** 4 de noviembre de 2026.

## Contexto
Follow Motors es un servicio de **asistencia y posventa para propietarios admitidos por la empresa**, no un ecommerce abierto. Un cliente llega a la sucursal con una decisión previa; la empresa controla qué clientes y motocicletas incorpora.

## Decisión
1. **Sin autorregistro público.** Nadie puede crear su cuenta o añadir una motocicleta desde la web pública.
2. **Alta de sucursal.** Un asesor autorizado verifica identidad y propiedad, registra cliente y motocicleta y vincula las unidades admitidas. La gerencia tiene el alcance que determine la matriz de permisos.
3. **Portal por titularidad.** Un cliente autenticado ve únicamente sus motocicletas, kilometraje, citas e historial autorizado.
4. **Sin motocicletas vinculadas.** Presentar un estado vacío con indicación de acudir a sucursal; nunca mostrarle productos ajenos, sugerir unidades ni ofrecer alta autónoma.
5. **Catálogo público informativo de asistencia.** Podrá informar sobre servicios, condiciones, contacto y modelos publicados, pero no es una tienda ni promete inventario, reserva o compra.
6. **Seguridad autoritativa.** La API valida cada permiso y pertenencia. La creación/vinculación y cambios de titularidad quedan auditados.
7. **Mínimo acceso.** Un asesor no puede modificar permisos de gerencia ni consultar expedientes sin autorización.

## Flujos pendientes de definición por la empresa
Verificación presencial de identidad, entrega y recuperación de credenciales, aceptación o rechazo de altas, reventa/cambio de titularidad, reglas de suspensión y protección de datos. No presumir esos procesos como implementados.

## Impacto
- **US02:** acceso solo a cuentas habilitadas.
- **US04:** alta por asesor autorizado de sucursal; no por cliente.
- **US08:** listado filtrado por titularidad, con estado vacío si no hay unidades.
- **US09–US12:** web pública de asistencia, no ventas ni inscripción.
- **US21:** permisos de asesor y gerencia con pruebas negativas.
- **UI:** sin botón de registro ni compra.
- **Datos:** cuentas, clientes y motos relacionados pero distinguibles; auditoría de alta.

La planificación V5 permanece como **referencia histórica**, pero la presente decisión prevalece para el producto nuevo. Esta decisión no implica funciones ya construidas.

[Alcance operativo](../alcance-posventa.md) · [Backlog](../backlog-v5.md)
