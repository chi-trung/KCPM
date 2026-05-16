-- ============================================================
-- V8: Change service_area from JSON to VARCHAR
-- ============================================================

ALTER TABLE `enterprises` 
MODIFY COLUMN `service_area` VARCHAR(500) NULL;
