USE follow_motors_v3_dev;
-- Una garantía por moto en esta primera versión; sus decisiones se registran por separado.
CREATE TABLE garantias (
    id CHAR(36) NOT NULL,
    motocicleta_id CHAR(36) NOT NULL,
    estado ENUM('PENDIENTE_REVISION','VIGENTE','INVALIDADA','FINALIZADA','NO_RECONOCIDA')
        NOT NULL DEFAULT 'PENDIENTE_REVISION',
    fecha_inicio DATE NOT NULL,
    reconocida_por CHAR(36) NULL,
    reconocida_utc DATETIME(6) NULL,
    creada_utc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    UNIQUE KEY uq_garantias_moto (motocicleta_id),
    CONSTRAINT fk_garantias_moto FOREIGN KEY (motocicleta_id)
        REFERENCES motocicletas(id) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_garantias_reconoce FOREIGN KEY (reconocida_por)
        REFERENCES usuarios(id) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB;

-- Tipo CFMOTO: usa moto registrada y sin datos de marca externa.
-- Tipo EXTERNA: sólo datos básicos, sin crear moto ni cuenta de portal.
CREATE TABLE citas (
    id CHAR(36) NOT NULL,
    tipo_motocicleta ENUM('CFMOTO','EXTERNA') NOT NULL,
    motocicleta_id CHAR(36) NULL,
    marca_externa VARCHAR(100) NULL,
    modelo_externo VARCHAR(150) NULL,
    contacto_externo VARCHAR(150) NULL,
    telefono_externo VARCHAR(25) NULL,
    inicio_local DATETIME NOT NULL,
    zona_horaria VARCHAR(64) NOT NULL DEFAULT 'America/Mexico_City',
    duracion_minutos SMALLINT UNSIGNED NOT NULL DEFAULT 60,
    motivo VARCHAR(400) NOT NULL,
    estado ENUM('PROGRAMADA','CONFIRMADA','CANCELADA','REALIZADA')
        NOT NULL DEFAULT 'PROGRAMADA',
    registrada_por CHAR(36) NOT NULL,
    creada_utc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    KEY idx_citas_inicio (inicio_local, estado),
    KEY idx_citas_moto (motocicleta_id),
    CONSTRAINT ck_citas_duracion CHECK (duracion_minutos BETWEEN 15 AND 480),
    CONSTRAINT ck_citas_motivo CHECK (CHAR_LENGTH(TRIM(motivo)) > 0),
    CONSTRAINT ck_citas_tipo CHECK (
       (tipo_motocicleta = 'CFMOTO'
          AND motocicleta_id IS NOT NULL
          AND marca_externa IS NULL AND modelo_externo IS NULL)
       OR
       (tipo_motocicleta = 'EXTERNA'
          AND motocicleta_id IS NULL
          AND marca_externa IS NOT NULL AND CHAR_LENGTH(TRIM(marca_externa)) > 0
          AND modelo_externo IS NOT NULL AND CHAR_LENGTH(TRIM(modelo_externo)) > 0
          AND contacto_externo IS NOT NULL AND CHAR_LENGTH(TRIM(contacto_externo)) > 0)
    ),
    CONSTRAINT fk_citas_moto FOREIGN KEY (motocicleta_id)
        REFERENCES motocicletas(id) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_citas_actor FOREIGN KEY (registrada_por)
        REFERENCES usuarios(id) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB;

-- Trabajo: se registra antes de validarse. El historial de talleres no se limita a 3.
-- Sólo un trabajo APROBADO por moto y etapa de garantía; los rechazados pueden rehacerse.
CREATE TABLE trabajos (
    id CHAR(36) NOT NULL,
    motocicleta_id CHAR(36) NOT NULL,
    cita_id CHAR(36) NULL,
    tipo ENUM('GARANTIA','POST_GARANTIA','OTRO') NOT NULL,
    etapa_id BIGINT UNSIGNED NULL,
    mecanico_id CHAR(36) NOT NULL,
    fecha_realizacion DATE NOT NULL,
    kilometraje_informativo INT UNSIGNED NOT NULL,
    descripcion TEXT NOT NULL,
    estado ENUM('PENDIENTE','APROBADO','RECHAZADO') NOT NULL DEFAULT 'PENDIENTE',
    validador_id CHAR(36) NULL,
    fecha_validacion_utc DATETIME(6) NULL,
    dentro_plazo BOOLEAN NULL,
    observaciones VARCHAR(1500) NULL,
    etapa_aprobada_guard BIGINT UNSIGNED GENERATED ALWAYS AS (
        CASE WHEN estado = 'APROBADO' AND tipo = 'GARANTIA'
             THEN etapa_id ELSE NULL END
    ) STORED,
    PRIMARY KEY (id),
    UNIQUE KEY uq_trabajos_aprobacion_etapa (motocicleta_id, etapa_aprobada_guard),
    KEY idx_trabajos_moto_fecha (motocicleta_id, fecha_realizacion),
    KEY idx_trabajos_pendientes (estado, fecha_realizacion),
    KEY idx_trabajos_etapa (etapa_id),
    KEY idx_trabajos_cita (cita_id),
    CONSTRAINT ck_trabajos_descripcion CHECK (CHAR_LENGTH(TRIM(descripcion)) > 0),
    CONSTRAINT ck_trabajos_tipo CHECK (
        (tipo = 'GARANTIA' AND etapa_id IS NOT NULL)
        OR (tipo <> 'GARANTIA' AND etapa_id IS NULL)
    ),
    CONSTRAINT ck_trabajos_validacion CHECK (
        (estado = 'PENDIENTE' AND validador_id IS NULL AND fecha_validacion_utc IS NULL
           AND dentro_plazo IS NULL)
        OR (estado IN ('APROBADO','RECHAZADO')
           AND validador_id IS NOT NULL AND fecha_validacion_utc IS NOT NULL)
    ),
    CONSTRAINT fk_trabajos_moto FOREIGN KEY (motocicleta_id)
        REFERENCES motocicletas(id) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_trabajos_cita FOREIGN KEY (cita_id)
        REFERENCES citas(id) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_trabajos_etapa FOREIGN KEY (etapa_id)
        REFERENCES etapas_mantenimiento(id) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_trabajos_mecanico FOREIGN KEY (mecanico_id)
        REFERENCES usuarios(id) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_trabajos_validador FOREIGN KEY (validador_id)
        REFERENCES usuarios(id) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB;

-- Toda resolución de garantía debe conservar autor, motivo y momento.
CREATE TABLE decisiones_garantia (
    id CHAR(36) NOT NULL,
    garantia_id CHAR(36) NOT NULL,
    trabajo_id CHAR(36) NULL,
    decision ENUM('RECONOCER','MANTENER','INVALIDAR','NO_RECONOCER','FINALIZAR') NOT NULL,
    motivo VARCHAR(1000) NOT NULL,
    decidida_por CHAR(36) NOT NULL,
    decidida_utc DATETIME(6) NOT NULL,
    PRIMARY KEY (id),
    KEY idx_decisiones_garantia_fecha (garantia_id, decidida_utc),
    CONSTRAINT ck_decisiones_motivo CHECK (CHAR_LENGTH(TRIM(motivo)) > 0),
    CONSTRAINT fk_decisiones_garantia FOREIGN KEY (garantia_id)
        REFERENCES garantias(id) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_decisiones_trabajo FOREIGN KEY (trabajo_id)
        REFERENCES trabajos(id) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_decisiones_actor FOREIGN KEY (decidida_por)
        REFERENCES usuarios(id) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB;
