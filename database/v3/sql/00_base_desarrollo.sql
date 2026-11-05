-- Follow Motors / Modelo V3 de desarrollo
-- No modifica ni elimina la base follow_motors usada por la API V2.
-- Ejecutar en MySQL Server 8.0.16+ (recomendado 8.4).
-- Antes de ejecutar comprueba que no exista una base V3 con datos importantes.
CREATE DATABASE IF NOT EXISTS follow_motors_v3_dev
  CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
USE follow_motors_v3_dev;
SELECT DATABASE() AS base_activa, VERSION() AS version_servidor;
SHOW TABLES;
