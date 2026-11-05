USE follow_motors_v3_dev;
-- NO se insertan cuentas ni contraseñas de ejemplo.
-- La creación de usuarios debe pasar por el backend y aplicar hash seguro.
INSERT INTO roles (codigo, nombre) VALUES
 ('DIRECTOR','Dirección / Supervisión'),
 ('GERENTE','Gerencia'),
 ('ASESOR','Asesor de servicio'),
 ('MECANICO','Técnico de taller'),
 ('VENDEDOR','Ventas'),
 ('CAJA','Caja / cobros'),
 ('CLIENTE','Propietario con acceso')
ON DUPLICATE KEY UPDATE codigo = codigo;

INSERT INTO permisos (codigo, descripcion) VALUES
 ('usuarios.crear','Crear cuentas de acceso'),
 ('usuarios.editar','Corregir datos de usuarios'),
 ('clientes.registrar','Registrar clientes'),
 ('clientes.editar','Editar datos de clientes'),
 ('clientes.editar_propio','Editar nombre, teléfono y correo propios'),
 ('motocicletas.registrar','Registrar motocicletas CFMOTO'),
 ('motocicletas.ver','Consultar motocicletas autorizadas'),
 ('motocicletas.transferir','Transferir propietario con historial'),
 ('agenda.ver','Consultar agenda del personal'),
 ('agenda.crear','Programar citas'),
 ('agenda.modificar','Reprogramar o cancelar citas'),
 ('trabajos.registrar','Registrar trabajo de taller'),
 ('trabajos.ver','Consultar servicios autorizados'),
 ('trabajos.validar','Aprobar o rechazar trabajos'),
 ('garantias.ver','Consultar información de garantías'),
 ('garantias.decidir','Reconocer, mantener o invalidar garantías'),
 ('roles.asignar','Asignar o retirar roles'),
 ('roles.gestionar','Administrar permisos de roles'),
 ('auditoria.ver','Consultar bitácora'),
 ('notificaciones.ver','Consultar notificaciones pendientes')
ON DUPLICATE KEY UPDATE codigo = codigo;

-- Roles iniciales, a aprobar por el equipo. No se fijan permisos dentro del Controller.
INSERT INTO roles_permisos (rol_codigo, permiso_codigo)
SELECT 'DIRECTOR', codigo FROM permisos
WHERE codigo IN ('roles.asignar','roles.gestionar','auditoria.ver','notificaciones.ver','garantias.ver','motocicletas.ver','trabajos.ver')
ON DUPLICATE KEY UPDATE rol_codigo = rol_codigo;

INSERT INTO roles_permisos (rol_codigo, permiso_codigo)
SELECT 'GERENTE', codigo FROM permisos
WHERE codigo IN ('usuarios.crear','usuarios.editar','clientes.registrar','clientes.editar',
 'motocicletas.registrar','motocicletas.ver','motocicletas.transferir',
 'agenda.ver','agenda.crear','agenda.modificar',
 'trabajos.ver','trabajos.validar','garantias.ver','garantias.decidir')
ON DUPLICATE KEY UPDATE rol_codigo = rol_codigo;

INSERT INTO roles_permisos (rol_codigo, permiso_codigo)
SELECT 'ASESOR', codigo FROM permisos
WHERE codigo IN ('clientes.registrar','clientes.editar','motocicletas.registrar','motocicletas.ver',
 'agenda.ver','agenda.crear','agenda.modificar','trabajos.ver','garantias.ver')
ON DUPLICATE KEY UPDATE rol_codigo = rol_codigo;

INSERT INTO roles_permisos (rol_codigo, permiso_codigo)
SELECT 'MECANICO', codigo FROM permisos
WHERE codigo IN ('agenda.ver','motocicletas.ver','trabajos.registrar','trabajos.ver')
ON DUPLICATE KEY UPDATE rol_codigo = rol_codigo;

INSERT INTO roles_permisos (rol_codigo, permiso_codigo)
SELECT 'CLIENTE', codigo FROM permisos
WHERE codigo IN ('clientes.editar_propio','motocicletas.ver','trabajos.ver','garantias.ver')
ON DUPLICATE KEY UPDATE rol_codigo = rol_codigo;

-- Versión congelada de reglas: desde la FECHA DE VENTA (3, 6 y 12 meses).
-- Se puede crear un NUEVO plan cuando la agencia cambie la política.
INSERT INTO planes_mantenimiento (codigo, nombre, meses_posteriores)
VALUES ('CFMOTO_2026_V1','CFMOTO: tres servicios obligatorios (2026)',6)
ON DUPLICATE KEY UPDATE codigo = codigo;

INSERT INTO etapas_mantenimiento (plan_id, numero_etapa, meses_desde_venta, dias_aviso, nombre)
SELECT id, 1, 3, 30, 'Primer servicio' FROM planes_mantenimiento WHERE codigo='CFMOTO_2026_V1'
ON DUPLICATE KEY UPDATE numero_etapa = numero_etapa;
INSERT INTO etapas_mantenimiento (plan_id, numero_etapa, meses_desde_venta, dias_aviso, nombre)
SELECT id, 2, 6, 30, 'Segundo servicio' FROM planes_mantenimiento WHERE codigo='CFMOTO_2026_V1'
ON DUPLICATE KEY UPDATE numero_etapa = numero_etapa;
INSERT INTO etapas_mantenimiento (plan_id, numero_etapa, meses_desde_venta, dias_aviso, nombre)
SELECT id, 3, 12, 30, 'Tercer servicio' FROM planes_mantenimiento WHERE codigo='CFMOTO_2026_V1'
ON DUPLICATE KEY UPDATE numero_etapa = numero_etapa;

SELECT p.codigo, e.numero_etapa, e.meses_desde_venta, p.meses_posteriores
FROM planes_mantenimiento p
JOIN etapas_mantenimiento e ON e.plan_id=p.id
WHERE p.codigo='CFMOTO_2026_V1'
ORDER BY e.numero_etapa;
