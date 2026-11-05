USE follow_motors_v3_dev;
-- Los UUID se asignan desde .NET; VARCHAR/CHAR(36) preserva compatibilidad con GUID.
-- La desactivación sustituye la eliminación física de personas.
CREATE TABLE usuarios (
    id CHAR(36) NOT NULL,
    nombre VARCHAR(150) NOT NULL,
    correo VARCHAR(254) NOT NULL,
    hash_password VARCHAR(255) NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    creado_utc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    UNIQUE KEY uq_usuarios_correo (correo),
    CONSTRAINT ck_usuarios_nombre CHECK (CHAR_LENGTH(TRIM(nombre)) > 0),
    CONSTRAINT ck_usuarios_correo CHECK (CHAR_LENGTH(TRIM(correo)) > 3),
    CONSTRAINT ck_usuarios_hash CHECK (CHAR_LENGTH(TRIM(hash_password)) > 0)
) ENGINE=InnoDB;

CREATE TABLE roles (
    codigo VARCHAR(50) NOT NULL,
    nombre VARCHAR(120) NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (codigo),
    CONSTRAINT ck_roles_codigo CHECK (CHAR_LENGTH(TRIM(codigo)) > 0)
) ENGINE=InnoDB;

CREATE TABLE permisos (
    codigo VARCHAR(90) NOT NULL,
    descripcion VARCHAR(200) NOT NULL,
    PRIMARY KEY (codigo),
    CONSTRAINT ck_permisos_codigo CHECK (CHAR_LENGTH(TRIM(codigo)) > 0)
) ENGINE=InnoDB;

-- Una cuenta puede tener varios roles, sin duplicarlos.
CREATE TABLE usuarios_roles (
    usuario_id CHAR(36) NOT NULL,
    rol_codigo VARCHAR(50) NOT NULL,
    PRIMARY KEY (usuario_id, rol_codigo),
    CONSTRAINT fk_usuarios_roles_usuario FOREIGN KEY (usuario_id)
        REFERENCES usuarios(id) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_usuarios_roles_rol FOREIGN KEY (rol_codigo)
        REFERENCES roles(codigo) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE roles_permisos (
    rol_codigo VARCHAR(50) NOT NULL,
    permiso_codigo VARCHAR(90) NOT NULL,
    PRIMARY KEY (rol_codigo, permiso_codigo),
    CONSTRAINT fk_roles_permisos_rol FOREIGN KEY (rol_codigo)
        REFERENCES roles(codigo) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_roles_permisos_permiso FOREIGN KEY (permiso_codigo)
        REFERENCES permisos(codigo) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB;

-- La persona puede ser cliente aunque no tenga acceso al portal.
-- El correo de contacto NO es clave única: una familia puede compartirlo.
-- Si el cliente recibe cuenta, usuario_id la enlaza sin duplicar identidad.
CREATE TABLE clientes (
    id CHAR(36) NOT NULL,
    usuario_id CHAR(36) NULL,
    nombre VARCHAR(150) NOT NULL,
    telefono VARCHAR(25) NULL,
    correo_contacto VARCHAR(254) NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    registrado_por CHAR(36) NULL,
    creado_utc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    UNIQUE KEY uq_clientes_usuario (usuario_id),
    KEY idx_clientes_nombre (nombre),
    CONSTRAINT ck_clientes_nombre CHECK (CHAR_LENGTH(TRIM(nombre)) > 0),
    CONSTRAINT fk_clientes_usuario FOREIGN KEY (usuario_id)
        REFERENCES usuarios(id) ON DELETE RESTRICT ON UPDATE RESTRICT,
    CONSTRAINT fk_clientes_registrador FOREIGN KEY (registrado_por)
        REFERENCES usuarios(id) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB;
