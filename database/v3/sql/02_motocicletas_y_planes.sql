USE follow_motors_v3_dev;
-- Cada plan es una versión histórica: no editar sus intervalos si ya está asignado.
CREATE TABLE planes_mantenimiento (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    codigo VARCHAR(40) NOT NULL,
    nombre VARCHAR(150) NOT NULL,
    meses_posteriores SMALLINT UNSIGNED NOT NULL DEFAULT 6,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    creado_utc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    UNIQUE KEY uq_planes_codigo (codigo),
    CONSTRAINT ck_planes_meses CHECK (meses_posteriores BETWEEN 1 AND 60)
) ENGINE=InnoDB;

CREATE TABLE etapas_mantenimiento (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    plan_id BIGINT UNSIGNED NOT NULL,
    numero_etapa SMALLINT UNSIGNED NOT NULL,
    meses_desde_venta SMALLINT UNSIGNED NOT NULL,
    dias_aviso SMALLINT UNSIGNED NOT NULL DEFAULT 30,
    nombre VARCHAR(100) NOT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_etapa_numero (plan_id, numero_etapa),
    UNIQUE KEY uq_etapa_mes (plan_id, meses_desde_venta),
    CONSTRAINT ck_etapa_numero CHECK (numero_etapa BETWEEN 1 AND 20),
    CONSTRAINT ck_etapa_mes CHECK (meses_desde_venta BETWEEN 1 AND 240),
    CONSTRAINT ck_etapa_aviso CHECK (dias_aviso <= 365),
    CONSTRAINT fk_etapas_plan FOREIGN KEY (plan_id)
        REFERENCES planes_mantenimiento(id) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB;

-- Solo CFMOTO se da de alta en el portal de motocicletas.
-- Otras marcas se registran únicamente en el módulo de agenda.
CREATE TABLE motocicletas (
    id CHAR(36) NOT NULL,
    cliente_actual_id CHAR(36) NOT NULL,
    plan_id BIGINT UNSIGNED NOT NULL,
    vin VARCHAR(32) NOT NULL,
    marca VARCHAR(30) NOT NULL DEFAULT 'CFMOTO',
    modelo VARCHAR(150) NOT NULL,
    fecha_venta DATE NOT NULL,
    origen ENUM('FOLLOW_MOTORS','OTRA_AGENCIA') NOT NULL,
    agencia_origen VARCHAR(150) NULL,
    estado ENUM('ACTIVA','INACTIVA') NOT NULL DEFAULT 'ACTIVA',
    registrada_por CHAR(36) NOT NULL,
    creada_utc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    UNIQUE KEY uq_motocicletas_vin (vin),
    KEY idx_motos_cliente (cliente_actual_id),
    KEY idx_motos_plan (plan_id),
    CONSTRAINT ck_motos_marca CHECK (marca = 'CFMOTO'),
    CONSTRAINT ck_motos_vin CHECK (CHAR_LENGTH(TRIM(vin)) >= 8),
    CONSTRAINT ck_motos_modelo CHECK (CHAR_LENGTH(TRIM(modelo)) > 0),
    CONSTRAINT fk_motos_cliente FOREIGN KEY (cliente_actual_id)
        REFERENCES clientes(id) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_motos_plan FOREIGN KEY (plan_id)
        REFERENCES planes_mantenimiento(id) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_motos_usuario FOREIGN KEY (registrada_por)
        REFERENCES usuarios(id) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB;

-- Sólo un propietario actual (fecha_fin NULL) por motocicleta.
-- La columna generada tiene NULL en filas históricas; UNIQUE admite varios NULL.
CREATE TABLE historial_propietarios (
    id CHAR(36) NOT NULL,
    motocicleta_id CHAR(36) NOT NULL,
    cliente_id CHAR(36) NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NULL,
    motivo VARCHAR(300) NULL,
    registrado_por CHAR(36) NOT NULL,
    propietario_actual_guard TINYINT GENERATED ALWAYS AS
        (CASE WHEN fecha_fin IS NULL THEN 1 ELSE NULL END) STORED,
    PRIMARY KEY (id),
    UNIQUE KEY uq_un_propietario_actual (motocicleta_id, propietario_actual_guard),
    KEY idx_historial_cliente (cliente_id),
    CONSTRAINT ck_historial_fechas CHECK (fecha_fin IS NULL OR fecha_fin >= fecha_inicio),
    CONSTRAINT fk_historial_moto FOREIGN KEY (motocicleta_id)
        REFERENCES motocicletas(id) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_historial_cliente FOREIGN KEY (cliente_id)
        REFERENCES clientes(id) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_historial_actor FOREIGN KEY (registrado_por)
        REFERENCES usuarios(id) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB;
