-- FollowMotors | Modelo relacional v1.0 | MySQL 8.0.16+
-- Proyecto academico de taller de motocicletas multimarca.
-- Diseno de base vacia; sin datos de prueba, DROP ni credenciales.
-- Variante de importacion para la base ya creada en Hostinger.
USE `u899160275_follow_motors`;

-- E01: Identidad y expedientes
CREATE TABLE IF NOT EXISTS usuarios (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    identificador VARCHAR(120) NOT NULL,
    hash_contrasena VARCHAR(255) NOT NULL,
    rol ENUM('GERENTE','EMPLEADO','CLIENTE') NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    creado_en DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    actualizado_en DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3) ON UPDATE CURRENT_TIMESTAMP(3),
    PRIMARY KEY (id),
    UNIQUE KEY uq_usuarios_identificador (identificador)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS clientes (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    usuario_id BIGINT UNSIGNED NOT NULL,
    nombres VARCHAR(120) NOT NULL,
    apellidos VARCHAR(120) NOT NULL,
    telefono VARCHAR(25) NULL,
    registrado_por_usuario_id BIGINT UNSIGNED NULL,
    creado_en DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    PRIMARY KEY (id),
    UNIQUE KEY uq_clientes_usuario (usuario_id),
    KEY idx_clientes_apellidos (apellidos, nombres),
    CONSTRAINT fk_clientes_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id),
    CONSTRAINT fk_clientes_registrador FOREIGN KEY (registrado_por_usuario_id) REFERENCES usuarios(id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS empleados (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    usuario_id BIGINT UNSIGNED NOT NULL,
    nombres VARCHAR(120) NOT NULL,
    apellidos VARCHAR(120) NOT NULL,
    es_mecanico BOOLEAN NOT NULL DEFAULT FALSE,
    disponible BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (id),
    UNIQUE KEY uq_empleados_usuario (usuario_id),
    CONSTRAINT fk_empleados_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS motocicletas (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    cliente_id BIGINT UNSIGNED NOT NULL,
    numero_serie VARCHAR(60) NOT NULL,
    placa VARCHAR(20) NULL,
    marca VARCHAR(80) NOT NULL,
    modelo VARCHAR(100) NOT NULL,
    anio SMALLINT UNSIGNED NULL,
    color VARCHAR(60) NULL,
    kilometraje_actual INT UNSIGNED NOT NULL DEFAULT 0,
    registrada_en DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    PRIMARY KEY (id),
    UNIQUE KEY uq_motocicletas_serie (numero_serie),
    KEY idx_motocicletas_cliente (cliente_id),
    CONSTRAINT fk_motocicletas_cliente FOREIGN KEY (cliente_id) REFERENCES clientes(id),
    CONSTRAINT chk_motocicletas_anio CHECK (anio IS NULL OR (anio BETWEEN 1900 AND 2150))
) ENGINE=InnoDB;

-- E02: Agenda, recepcion y diagnostico
CREATE TABLE IF NOT EXISTS franjas_citas (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    inicio DATETIME(3) NOT NULL,
    fin DATETIME(3) NOT NULL,
    capacidad SMALLINT UNSIGNED NOT NULL DEFAULT 1,
    habilitada BOOLEAN NOT NULL DEFAULT TRUE,
    creada_por_usuario_id BIGINT UNSIGNED NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_franjas_inicio_fin (inicio, fin),
    CONSTRAINT fk_franjas_creador FOREIGN KEY (creada_por_usuario_id) REFERENCES usuarios(id),
    CONSTRAINT chk_franjas_duracion CHECK (fin > inicio),
    CONSTRAINT chk_franjas_capacidad CHECK (capacidad >= 1)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS citas (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    franja_id BIGINT UNSIGNED NOT NULL,
    cliente_id BIGINT UNSIGNED NOT NULL,
    motocicleta_id BIGINT UNSIGNED NULL,
    motivo VARCHAR(400) NOT NULL,
    origen ENUM('WEB','PRESENCIAL') NOT NULL,
    estado ENUM('CONFIRMADA','ATENDIDA','CANCELADA','NO_ASISTIO') NOT NULL DEFAULT 'CONFIRMADA',
    registrada_en DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    PRIMARY KEY (id),
    KEY idx_citas_franja_estado (franja_id, estado),
    KEY idx_citas_cliente_estado (cliente_id, estado),
    CONSTRAINT fk_citas_franja FOREIGN KEY (franja_id) REFERENCES franjas_citas(id),
    CONSTRAINT fk_citas_cliente FOREIGN KEY (cliente_id) REFERENCES clientes(id),
    CONSTRAINT fk_citas_moto FOREIGN KEY (motocicleta_id) REFERENCES motocicletas(id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS tarifas_revision (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    importe DECIMAL(12,2) NOT NULL,
    vigente_desde DATETIME(3) NOT NULL,
    vigente_hasta DATETIME(3) NULL,
    PRIMARY KEY (id),
    CONSTRAINT chk_tarifas_importe CHECK (importe >= 0),
    CONSTRAINT chk_tarifas_periodo CHECK (vigente_hasta IS NULL OR vigente_hasta > vigente_desde)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS atenciones (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    cliente_id BIGINT UNSIGNED NOT NULL,
    motocicleta_id BIGINT UNSIGNED NOT NULL,
    cita_id BIGINT UNSIGNED NULL,
    mecanico_id BIGINT UNSIGNED NOT NULL,
    motivo TEXT NOT NULL,
    kilometraje_ingreso INT UNSIGNED NOT NULL,
    diagnostico TEXT NULL,
    estado ENUM('RECIBIDA','DIAGNOSTICADA','SIN_REPARACION','CONTRATADA','CERRADA') NOT NULL DEFAULT 'RECIBIDA',
    ingresada_en DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    diagnosticada_en DATETIME(3) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_atenciones_cita (cita_id),
    KEY idx_atenciones_moto (motocicleta_id),
    KEY idx_atenciones_mecanico_estado (mecanico_id, estado),
    CONSTRAINT fk_atenciones_cliente FOREIGN KEY (cliente_id) REFERENCES clientes(id),
    CONSTRAINT fk_atenciones_moto FOREIGN KEY (motocicleta_id) REFERENCES motocicletas(id),
    CONSTRAINT fk_atenciones_cita FOREIGN KEY (cita_id) REFERENCES citas(id),
    CONSTRAINT fk_atenciones_mecanico FOREIGN KEY (mecanico_id) REFERENCES empleados(id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS cobros_revision (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    atencion_id BIGINT UNSIGNED NOT NULL,
    tarifa_id BIGINT UNSIGNED NOT NULL,
    importe_cobrado DECIMAL(12,2) NOT NULL,
    registrado_por_usuario_id BIGINT UNSIGNED NOT NULL,
    cobrado_en DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    referencia_comprobante VARCHAR(120) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_cobros_revision_atencion (atencion_id),
    CONSTRAINT fk_cobros_revision_atencion FOREIGN KEY (atencion_id) REFERENCES atenciones(id),
    CONSTRAINT fk_cobros_revision_tarifa FOREIGN KEY (tarifa_id) REFERENCES tarifas_revision(id),
    CONSTRAINT fk_cobros_revision_usuario FOREIGN KEY (registrado_por_usuario_id) REFERENCES usuarios(id),
    CONSTRAINT chk_cobros_revision_monto CHECK (importe_cobrado >= 0)
) ENGINE=InnoDB;

-- E03: Presupuestos, contratos, piezas, anticipos y garantias
CREATE TABLE IF NOT EXISTS tipos_servicio (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    nombre VARCHAR(130) NOT NULL,
    descripcion TEXT NULL,
    garantia_dias SMALLINT UNSIGNED NOT NULL DEFAULT 0,
    cobertura TEXT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (id),
    UNIQUE KEY uq_tipos_servicio_nombre (nombre)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS presupuestos (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    atencion_id BIGINT UNSIGNED NOT NULL,
    tipo_servicio_id BIGINT UNSIGNED NOT NULL,
    descripcion_trabajo TEXT NOT NULL,
    mano_obra_acordada DECIMAL(12,2) NOT NULL,
    refacciones_acordadas DECIMAL(12,2) NOT NULL DEFAULT 0,
    total_acordado DECIMAL(12,2) GENERATED ALWAYS AS (mano_obra_acordada + refacciones_acordadas) STORED,
    entrega_estimada DATETIME(3) NULL,
    estado ENUM('BORRADOR','ACEPTADO','RECHAZADO') NOT NULL DEFAULT 'BORRADOR',
    registrado_por_usuario_id BIGINT UNSIGNED NOT NULL,
    acordado_en DATETIME(3) NULL,
    creado_en DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    PRIMARY KEY (id),
    UNIQUE KEY uq_presupuestos_atencion (atencion_id),
    CONSTRAINT fk_presupuestos_atencion FOREIGN KEY (atencion_id) REFERENCES atenciones(id),
    CONSTRAINT fk_presupuestos_tipo FOREIGN KEY (tipo_servicio_id) REFERENCES tipos_servicio(id),
    CONSTRAINT fk_presupuestos_usuario FOREIGN KEY (registrado_por_usuario_id) REFERENCES usuarios(id),
    CONSTRAINT chk_presupuestos_manoobra CHECK (mano_obra_acordada >= 0),
    CONSTRAINT chk_presupuestos_refacciones CHECK (refacciones_acordadas >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS tareas_servicio (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    presupuesto_id BIGINT UNSIGNED NOT NULL,
    descripcion VARCHAR(300) NOT NULL,
    orden SMALLINT UNSIGNED NOT NULL,
    peso_porcentual DECIMAL(5,2) NOT NULL,
    avance_tarea DECIMAL(5,2) NOT NULL DEFAULT 0,
    estado ENUM('PENDIENTE','EN_PROCESO','COMPLETADA') NOT NULL DEFAULT 'PENDIENTE',
    evidencia TEXT NULL,
    actualizada_en DATETIME(3) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_tareas_presupuesto_orden (presupuesto_id, orden),
    CONSTRAINT fk_tareas_presupuesto FOREIGN KEY (presupuesto_id) REFERENCES presupuestos(id),
    CONSTRAINT chk_tareas_peso CHECK (peso_porcentual > 0 AND peso_porcentual <= 100),
    CONSTRAINT chk_tareas_avance CHECK (avance_tarea >= 0 AND avance_tarea <= 100)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS refacciones_servicio (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    presupuesto_id BIGINT UNSIGNED NOT NULL,
    descripcion VARCHAR(300) NOT NULL,
    cantidad_solicitada SMALLINT UNSIGNED NOT NULL,
    cantidad_recibida SMALLINT UNSIGNED NOT NULL DEFAULT 0,
    cantidad_utilizada SMALLINT UNSIGNED NOT NULL DEFAULT 0,
    cantidad_devuelta SMALLINT UNSIGNED NOT NULL DEFAULT 0,
    precio_unitario_cliente DECIMAL(12,2) NOT NULL,
    costo_unitario_proveedor DECIMAL(12,2) NULL,
    referencia_factura_proveedor VARCHAR(150) NULL,
    solicitada_en DATETIME(3) NULL,
    recibida_en DATETIME(3) NULL,
    devuelta_en DATETIME(3) NULL,
    PRIMARY KEY (id),
    KEY idx_refacciones_presupuesto (presupuesto_id),
    CONSTRAINT fk_refacciones_presupuesto FOREIGN KEY (presupuesto_id) REFERENCES presupuestos(id),
    CONSTRAINT chk_refacciones_cantidad CHECK (cantidad_solicitada >= 1 AND cantidad_recibida <= cantidad_solicitada),
    CONSTRAINT chk_refacciones_consumo CHECK (cantidad_utilizada + cantidad_devuelta <= cantidad_recibida),
    CONSTRAINT chk_refacciones_cliente CHECK (precio_unitario_cliente >= 0),
    CONSTRAINT chk_refacciones_proveedor CHECK (costo_unitario_proveedor IS NULL OR costo_unitario_proveedor >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ordenes_servicio (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    presupuesto_id BIGINT UNSIGNED NOT NULL,
    estado ENUM('PENDIENTE_PIEZAS','LISTA','EN_EJECUCION','CANCELADA','TERMINADA','ENTREGADA') NOT NULL DEFAULT 'LISTA',
    iniciada_en DATETIME(3) NULL,
    aviso_avance_25_en DATETIME(3) NULL,
    terminada_en DATETIME(3) NULL,
    entregada_en DATETIME(3) NULL,
    version INT UNSIGNED NOT NULL DEFAULT 1,
    creada_en DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    PRIMARY KEY (id),
    UNIQUE KEY uq_ordenes_presupuesto (presupuesto_id),
    CONSTRAINT fk_ordenes_presupuesto FOREIGN KEY (presupuesto_id) REFERENCES presupuestos(id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS anticipos_reparacion (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    orden_id BIGINT UNSIGNED NOT NULL,
    monto_mano_obra DECIMAL(12,2) NOT NULL,
    monto_refacciones DECIMAL(12,2) NOT NULL,
    total_pagado DECIMAL(12,2) GENERATED ALWAYS AS (monto_mano_obra + monto_refacciones) STORED,
    referencia_comprobante VARCHAR(120) NULL,
    registrado_por_usuario_id BIGINT UNSIGNED NOT NULL,
    pagado_en DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    PRIMARY KEY (id),
    UNIQUE KEY uq_anticipos_orden (orden_id),
    CONSTRAINT fk_anticipos_orden FOREIGN KEY (orden_id) REFERENCES ordenes_servicio(id),
    CONSTRAINT fk_anticipos_usuario FOREIGN KEY (registrado_por_usuario_id) REFERENCES usuarios(id),
    CONSTRAINT chk_anticipos_manoobra CHECK (monto_mano_obra >= 0),
    CONSTRAINT chk_anticipos_refacciones CHECK (monto_refacciones >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS garantias_servicio (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    orden_id BIGINT UNSIGNED NOT NULL,
    aplica BOOLEAN NOT NULL,
    duracion_dias SMALLINT UNSIGNED NOT NULL DEFAULT 0,
    condiciones TEXT NOT NULL,
    informada_en DATETIME(3) NOT NULL,
    inicia_en DATETIME(3) NULL,
    vence_en DATETIME(3) NULL,
    referencia_comprobante VARCHAR(120) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_garantias_orden (orden_id),
    CONSTRAINT fk_garantias_orden FOREIGN KEY (orden_id) REFERENCES ordenes_servicio(id),
    CONSTRAINT chk_garantias_fechas CHECK (vence_en IS NULL OR (inicia_en IS NOT NULL AND vence_en >= inicia_en))
) ENGINE=InnoDB;

-- E04: Cancelaciones, seguimiento, mantenimiento, avisos y auditoria
CREATE TABLE IF NOT EXISTS solicitudes_cancelacion (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    orden_id BIGINT UNSIGNED NOT NULL,
    cliente_id BIGINT UNSIGNED NOT NULL,
    estado ENUM('SOLICITADA','APROBADA','RECHAZADA') NOT NULL DEFAULT 'SOLICITADA',
    progreso_al_resolver DECIMAL(5,2) NULL,
    costo_mano_obra_realizada DECIMAL(12,2) NULL,
    costo_refacciones_usadas DECIMAL(12,2) NULL,
    monto_reembolsado DECIMAL(12,2) NULL,
    motivo_rechazo VARCHAR(300) NULL,
    solicitada_en DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    resuelta_en DATETIME(3) NULL,
    reembolsada_en DATETIME(3) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_cancelaciones_orden (orden_id),
    CONSTRAINT fk_cancelaciones_orden FOREIGN KEY (orden_id) REFERENCES ordenes_servicio(id),
    CONSTRAINT fk_cancelaciones_cliente FOREIGN KEY (cliente_id) REFERENCES clientes(id),
    CONSTRAINT chk_cancelaciones_progreso CHECK (progreso_al_resolver IS NULL OR (progreso_al_resolver BETWEEN 0 AND 100)),
    CONSTRAINT chk_cancelaciones_manoobra CHECK (costo_mano_obra_realizada IS NULL OR costo_mano_obra_realizada >= 0),
    CONSTRAINT chk_cancelaciones_piezas CHECK (costo_refacciones_usadas IS NULL OR costo_refacciones_usadas >= 0),
    CONSTRAINT chk_cancelaciones_reembolso CHECK (monto_reembolsado IS NULL OR monto_reembolsado >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS mantenimientos_programados (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    motocicleta_id BIGINT UNSIGNED NOT NULL,
    orden_origen_id BIGINT UNSIGNED NULL,
    descripcion VARCHAR(300) NOT NULL,
    fecha_estimada DATE NULL,
    kilometraje_objetivo INT UNSIGNED NULL,
    estado ENUM('PENDIENTE','REALIZADO','CANCELADO') NOT NULL DEFAULT 'PENDIENTE',
    PRIMARY KEY (id),
    KEY idx_mantenimientos_moto_estado (motocicleta_id, estado),
    CONSTRAINT fk_mantenimientos_moto FOREIGN KEY (motocicleta_id) REFERENCES motocicletas(id),
    CONSTRAINT fk_mantenimientos_orden FOREIGN KEY (orden_origen_id) REFERENCES ordenes_servicio(id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS notificaciones (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    cliente_id BIGINT UNSIGNED NOT NULL,
    orden_id BIGINT UNSIGNED NULL,
    cita_id BIGINT UNSIGNED NULL,
    tipo ENUM('CITA_CONFIRMADA','AVANCE_25','SERVICIO_TERMINADO','CANCELACION_RESUELTA') NOT NULL,
    mensaje VARCHAR(400) NOT NULL,
    creada_en DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    leida_en DATETIME(3) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_notificaciones_orden_tipo (orden_id, tipo),
    KEY idx_notificaciones_cliente (cliente_id, leida_en),
    CONSTRAINT fk_notificaciones_cliente FOREIGN KEY (cliente_id) REFERENCES clientes(id),
    CONSTRAINT fk_notificaciones_orden FOREIGN KEY (orden_id) REFERENCES ordenes_servicio(id),
    CONSTRAINT fk_notificaciones_cita FOREIGN KEY (cita_id) REFERENCES citas(id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS auditoria (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    usuario_id BIGINT UNSIGNED NULL,
    entidad VARCHAR(80) NOT NULL,
    registro_id BIGINT UNSIGNED NOT NULL,
    accion VARCHAR(80) NOT NULL,
    antes JSON NULL,
    despues JSON NULL,
    registrada_en DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    PRIMARY KEY (id),
    KEY idx_auditoria_entidad (entidad, registro_id),
    CONSTRAINT fk_auditoria_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
) ENGINE=InnoDB;
-- Sesiones revocables. Esta extensión también se puede ejecutar sobre una instalación existente.
CREATE TABLE IF NOT EXISTS sesiones_usuarios (
    id BINARY(16) NOT NULL,
    usuario_id BIGINT UNSIGNED NOT NULL,
    expira_en DATETIME(3) NOT NULL,
    PRIMARY KEY (id),
    KEY idx_sesiones_expira (expira_en),
    CONSTRAINT fk_sesiones_usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
) ENGINE=InnoDB;
