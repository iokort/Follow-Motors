# 01 — Reglas de negocio

Los códigos `NEG` se utilizan en historias, pruebas y decisiones. Se evita la numeración provisional de conversaciones anteriores. **C** = confirmado explícitamente por el negocio; **D** = criterio de diseño para completar y poder implementar el flujo (revisable).

| ID | Tipo | Regla |
| --- | --- | --- |
| NEG-01 | C | FollowMotors es un taller multimarca, no vende motocicletas. |
| NEG-02 | C | Solo gerente y empleados autorizados crean cuentas, en sucursal y sin cita previa. |
| NEG-03 | C | No existe registro público; la cuenta puede crearse sin contratación ni moto registrada. |
| NEG-04 | C | Solo el gerente puede desactivar cuentas de clientes. |
| NEG-05 | C | Cada cliente usa un identificador tipo `nombre@follow.motors`; el dominio es ficticio y no recibe correos reales. |
| NEG-06 | C | Un cliente puede tener varias motos; solo consulta las suyas. |
| NEG-07 | D | VIN o identificador de chasis evita duplicados de unidades; vinculación o corrección requiere intervención autorizada. |
| NEG-08 | C | Portal Angular consulta moto, servicios, próximos mantenimientos, garantía, citas y trabajos. |
| NEG-09 | C | Clientes activos pueden reservar citas en línea con confirmación automática de un horario disponible. |
| NEG-10 | C | Personas sin cuenta acuden a sucursal; el registro no requiere cita. |
| NEG-11 | D | El personal configura cupos de recepción/revisión; la API reserva el cupo en una operación atómica. |
| NEG-12 | C | Contrato, revisión, acuerdo de precios y autorización de reparaciones solo presencialmente. |
| NEG-13 | C | La revisión/diagnóstico tiene precio fijo y se paga el mismo día, se acepte o no la reparación. |
| NEG-14 | C | El mecánico fija la mano de obra después del diagnóstico según la moto y la intervención necesaria. |
| NEG-15 | C | La mano de obra requiere aceptación presencial y su precio acordado se respeta. |
| NEG-16 | C | Las refacciones se adquieren por proveedor externo sin margen; se cobran aparte de mano de obra. |
| NEG-17 | C | Si el proveedor cambia el costo durante un servicio ya acordado, el precio al cliente se respeta. |
| NEG-18 | C | El anticipo cubre la reparación (mano de obra + piezas); el diagnóstico no se vuelve a sumar. |
| NEG-19 | C | El comprobante con las condiciones de garantía del servicio se entrega al pagar el anticipo. |
| NEG-20 | C | Las garantías son del taller sobre servicios y reparaciones, no de fabricantes; duración y cobertura dependen del tipo de trabajo. |
| NEG-21 | D | La vigencia de garantía comienza con el cierre/entrega; las condiciones ya quedaron pactadas por anticipado. |
| NEG-22 | C | Una atención activa tiene exactamente un mecánico responsable; un mecánico puede tener varias atenciones. |
| NEG-23 | C | El sistema asigna automáticamente el responsable según menor carga de trabajo. |
| NEG-24 | D | La carga inicial se mide por cantidad de atenciones abiertas; desempate por turno/identificador. Asignación transaccional para evitar carreras. |
| NEG-25 | C | Si faltan refacciones, la atención espera y el mecánico puede continuar otra asignación. |
| NEG-26 | C | Las refacciones no utilizadas permanecen selladas y se devuelven al proveedor; el proveedor no usa el sistema. |
| NEG-27 | C | Cada reparación se descompone en tareas claras, ponderadas para sumar 100 % del trabajo. |
| NEG-28 | D | El avance total suma tareas terminadas y avance verificable de la tarea en curso, sin basarse en la fecha prometida. |
| NEG-29 | C | Se comunica al cliente que la reparación ya avanzó cuando se alcanza al menos 25 % de trabajo real. |
| NEG-30 | C | La fecha prevista de entrega es informativa y la estima el mecánico; no sustituye progreso de tareas. |
| NEG-31 | C | El cliente puede solicitar cancelar una reparación con avance de hasta 60 %, inclusive; por encima de 60 % se concluye el servicio y no se cancela. |
| NEG-32 | C | Si cancela dentro del límite, el cliente paga mano de obra efectivamente realizada y refacciones usadas; el resto de su anticipo se devuelve. |
| NEG-33 | C | El diagnóstico se pagó por separado y no es reembolsable. |
| NEG-34 | D | La API fija el avance y el consumo de piezas en el instante de resolver una cancelación, impide nuevas tareas y registra importes/auditoría. |
| NEG-35 | D | La cancelación antes de empezar tareas es posible con devolución íntegra del anticipo de reparación, si no hay piezas usadas. |
| NEG-36 | C | Solo servicios completados forman garantía ejecutable; el historial de revisiones y cancelaciones también permanece consultable. |
| NEG-37 | D | El personal registra la entrega y el cierre; el cliente consulta fecha, tareas, refacciones y estado de garantía. |
| NEG-38 | D | Acciones sensibles quedan auditadas; expedientes históricos no se eliminan físicamente al desactivar una cuenta. |

### Condiciones de cálculo

`Avance total = Σ(peso_tarea × fracción_ejecutada_verificada)`, acotado a 0–100. El peso total del plan aprobado es 100.

Para una cancelación autorizada (avance ≤ 60):

`Retención = mano_obra_acordada × fracción_de_trabajo_ejecutada + costo_de_piezas_usadas`

`Reembolso = anticipo_reparación − retención`, acotado a [0, anticipo]. Las piezas no usadas se devuelven selladas al proveedor; no cuentan como consumidas. El diagnóstico **nunca** entra en la base del reembolso.

Una cancelación solicitada con avance superior a 60 % **no se concede**: el taller **termina y entrega la reparación**, no retiene el anticipo a cambio de una moto incompleta.

### Precisión sobre las tareas

Para que el progreso refleje trabajo real, se recomienda definir tareas pequeñas y observables. Una tarea puede tener puntos de verificación parciales documentados; no se aceptan porcentajes arbitrarios sin registro. El plan de tareas se fija al autorizar el presupuesto y los cambios requieren una corrección trazable.
