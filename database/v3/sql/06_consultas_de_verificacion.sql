USE follow_motors_v3_dev;
-- Comprobaciones de lectura: ninguna consulta altera información.
SELECT DATABASE() AS base_activa, VERSION() AS version_servidor;
SHOW TABLES;
SELECT COUNT(*) AS total_tablas FROM information_schema.tables
 WHERE table_schema = DATABASE() AND table_type = 'BASE TABLE';
SELECT table_name, constraint_name, constraint_type
 FROM information_schema.table_constraints
 WHERE constraint_schema = DATABASE()
 ORDER BY table_name, constraint_type, constraint_name;
-- Debe devolver 3, 6 y 12 meses.
SELECT p.codigo, e.numero_etapa, e.meses_desde_venta, e.dias_aviso
 FROM planes_mantenimiento p
 JOIN etapas_mantenimiento e ON e.plan_id = p.id
 WHERE p.codigo = 'CFMOTO_2026_V1'
 ORDER BY e.numero_etapa;
-- Ejemplo de fechas, sin necesidad de cargar una moto.
SELECT e.numero_etapa, e.meses_desde_venta,
 DATE_ADD(DATE('2026-01-15'), INTERVAL e.meses_desde_venta MONTH) AS fecha_programada
 FROM etapas_mantenimiento e
 JOIN planes_mantenimiento p ON p.id=e.plan_id
 WHERE p.codigo='CFMOTO_2026_V1'
 ORDER BY e.numero_etapa;
-- FKs verificables en Workbench.
SELECT table_name, column_name, referenced_table_name, referenced_column_name
 FROM information_schema.key_column_usage
 WHERE table_schema = DATABASE() AND referenced_table_name IS NOT NULL
 ORDER BY table_name, column_name;
