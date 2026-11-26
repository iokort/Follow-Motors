// Responsabilidad: Atender el comando para aceptar o rechazar formalmente una solicitud de
// cancelación de orden.
// Consideraciones: Coordinar contratos y servicios de Application, aplicar reglas de Domain y
// persistir por abstracciones. Congelar estado en transacción, comprobar avance <= 60 %, calcular
// y registrar devolución.
// Historias relacionadas: US11
