USE follow_motors_v3_dev;
-- Prueba opcional después de ejecutar 00 a 05. Nunca crea credenciales utilizables.
-- Todos los registros de prueba se revierten con ROLLBACK.
START TRANSACTION;
SET @id_usuario = UUID();
SET @id_cliente = UUID();
SET @id_moto = UUID();
SET @id_cita_cfmoto = UUID();
SET @id_cita_externa = UUID();
INSERT INTO usuarios (id,nombre,correo,hash_password) VALUES
 (@id_usuario,'Empleado de prueba',CONCAT('test-',REPLACE(@id_usuario,'-',''),'@invalid.local'),'SOLO_PRUEBA_NO_ES_HASH_VALIDO');
INSERT INTO clientes (id,nombre,telefono,usuario_id,registrado_por) VALUES
 (@id_cliente,'Cliente de prueba','4420000000',NULL,@id_usuario);
INSERT INTO motocicletas
 (id,cliente_actual_id,plan_id,vin,marca,modelo,fecha_venta,origen,registrada_por)
 SELECT @id_moto,@id_cliente,id,CONCAT('TEST',LEFT(REPLACE(@id_moto,'-',''),13)),
  'CFMOTO','450SR','2026-01-15','FOLLOW_MOTORS',@id_usuario
 FROM planes_mantenimiento WHERE codigo='CFMOTO_2026_V1';
INSERT INTO historial_propietarios
 (id,motocicleta_id,cliente_id,fecha_inicio,registrado_por)
 VALUES (UUID(),@id_moto,@id_cliente,'2026-01-15',@id_usuario);
INSERT INTO garantias (id,motocicleta_id,estado,fecha_inicio) VALUES
 (UUID(),@id_moto,'VIGENTE','2026-01-15');
INSERT INTO citas (id,tipo_motocicleta,motocicleta_id,inicio_local,motivo,registrada_por)
 VALUES (@id_cita_cfmoto,'CFMOTO',@id_moto,'2026-11-15 09:00:00','Revisión general',@id_usuario);
INSERT INTO citas
 (id,tipo_motocicleta,marca_externa,modelo_externo,contacto_externo,telefono_externo,inicio_local,motivo,registrada_por)
 VALUES (@id_cita_externa,'EXTERNA','Honda','CB190R','Cliente externo','4421111111',
  '2026-11-15 10:00:00','Cita de taller',@id_usuario);
SELECT tipo_motocicleta, motocicleta_id, marca_externa, modelo_externo
 FROM citas WHERE id IN (@id_cita_cfmoto,@id_cita_externa);
SELECT m.modelo, g.estado AS estado_garantia
 FROM motocicletas m JOIN garantias g ON g.motocicleta_id=m.id
 WHERE m.id=@id_moto;
ROLLBACK;
-- Verificación: no queda ninguno de los datos temporales.
SELECT COUNT(*) AS citas_de_prueba_restantes FROM citas
 WHERE id IN (@id_cita_cfmoto,@id_cita_externa);
