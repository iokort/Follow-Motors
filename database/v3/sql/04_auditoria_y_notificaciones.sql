USE follow_motors_v3_dev;
-- Las acciones sensibles dejan rastro. entidad_id es referencia lógica polimórfica;
-- su existencia en la tabla destino se comprueba en el caso de uso.
CREATE TABLE auditoria (
    id CHAR(36) NOT NULL,
    actor_tipo ENUM('USUARIO','SISTEMA') NOT NULL,
    actor_id CHAR(36) NULL,
    accion VARCHAR(100) NOT NULL,
    entidad VARCHAR(90) NOT NULL,
    entidad_id CHAR(36) NOT NULL,
    detalle JSON NOT NULL,
    fecha_utc DATETIME(6) NOT NULL,
    PRIMARY KEY (id),
    KEY idx_auditoria_entidad (entidad, entidad_id, fecha_utc),
    KEY idx_auditoria_actor (actor_id, fecha_utc),
    CONSTRAINT ck_auditoria_actor CHECK (
        (actor_tipo = 'SISTEMA' AND actor_id IS NULL)
        OR (actor_tipo = 'USUARIO' AND actor_id IS NOT NULL)
    ),
    CONSTRAINT fk_auditoria_actor FOREIGN KEY (actor_id)
        REFERENCES usuarios(id) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB;

-- Outbox: el registro queda guardado aunque WhatsApp no esté disponible.
-- clave_idempotencia impide repetir el mismo evento lógico.
CREATE TABLE notificaciones_salida (
    id CHAR(36) NOT NULL,
    cliente_id CHAR(36) NOT NULL,
    evento VARCHAR(100) NOT NULL,
    medio ENUM('WHATSAPP','CORREO') NOT NULL DEFAULT 'WHATSAPP',
    contenido JSON NOT NULL,
    clave_idempotencia VARCHAR(150) NOT NULL,
    estado ENUM('PENDIENTE','EN_PROCESO','ENVIADA','ERROR')
        NOT NULL DEFAULT 'PENDIENTE',
    intentos SMALLINT UNSIGNED NOT NULL DEFAULT 0,
    ultimo_error VARCHAR(800) NULL,
    creada_utc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    procesada_utc DATETIME(6) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_outbox_idempotencia (clave_idempotencia),
    KEY idx_outbox_pendientes (estado, creada_utc),
    CONSTRAINT fk_outbox_cliente FOREIGN KEY (cliente_id)
        REFERENCES clientes(id) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB;
