SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

START TRANSACTION;

-- ============================================================
-- TABLES
-- (MySQL ENUM is declared inline per column — no separate type)
-- ============================================================

-- 1. users
CREATE TABLE IF NOT EXISTS `users` (
    `id`            CHAR(36)      NOT NULL DEFAULT (UUID()),
    `email`         VARCHAR(255)  NOT NULL,
    `password_hash` VARCHAR(255)  NOT NULL,
    `full_name`     VARCHAR(100)  NOT NULL,
    `phone`         VARCHAR(15)   NULL,
    `role`          ENUM('citizen','enterprise','collector','admin') NOT NULL,
    `district`      VARCHAR(100)  NULL,
    `ward`          VARCHAR(100)  NULL,
    `is_active`     TINYINT(1)    NOT NULL DEFAULT 1,
    `created_at`    DATETIME(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_users_email`  (`email`),
    UNIQUE KEY `uq_users_phone`  (`phone`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. enterprises
CREATE TABLE IF NOT EXISTS `enterprises` (
    `id`                  CHAR(36)      NOT NULL DEFAULT (UUID()),
    `user_id`             CHAR(36)      NOT NULL,
    `company_name`        VARCHAR(200)  NOT NULL,
    -- JSON replaces JSONB: [{district: "Quận 1", wards: ["Phường A"]}]
    `service_area`        JSON          NULL,
    `capacity_kg_per_day` INT           NULL,
    `is_verified`         TINYINT(1)    NOT NULL DEFAULT 0,
    `created_at`          DATETIME(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_enterprises_user_id` (`user_id`),
    CONSTRAINT `fk_enterprises_user`
        FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. waste_categories
CREATE TABLE IF NOT EXISTS `waste_categories` (
    `id`          INT           NOT NULL AUTO_INCREMENT,
    `name`        VARCHAR(50)   NOT NULL,
    `description` TEXT          NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_waste_categories_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 4. collectors
CREATE TABLE IF NOT EXISTS `collectors` (
    `id`            CHAR(36)    NOT NULL DEFAULT (UUID()),
    `user_id`       CHAR(36)    NOT NULL,
    `enterprise_id` CHAR(36)    NOT NULL,
    `is_available`  TINYINT(1)  NOT NULL DEFAULT 1,
    `created_at`    DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_collectors_user_id` (`user_id`),
    CONSTRAINT `fk_collectors_user`
        FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_collectors_enterprise`
        FOREIGN KEY (`enterprise_id`) REFERENCES `enterprises`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 5. enterprise_waste_types
CREATE TABLE IF NOT EXISTS `enterprise_waste_types` (
    `id`                CHAR(36) NOT NULL DEFAULT (UUID()),
    `enterprise_id`     CHAR(36) NOT NULL,
    `waste_category_id` INT      NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_enterprise_waste_type` (`enterprise_id`, `waste_category_id`),
    CONSTRAINT `fk_ewt_enterprise`
        FOREIGN KEY (`enterprise_id`) REFERENCES `enterprises`(`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_ewt_category`
        FOREIGN KEY (`waste_category_id`) REFERENCES `waste_categories`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 6. waste_reports
-- ⚠️ PostGIS removed: latitude + longitude DECIMAL used instead.
-- Haversine query example (radius 5km):
--   WHERE (6371000 * 2 * ASIN(SQRT(
--     POW(SIN(RADIANS(wr.latitude  - :lat) / 2), 2) +
--     COS(RADIANS(:lat)) * COS(RADIANS(wr.latitude)) *
--     POW(SIN(RADIANS(wr.longitude - :lon) / 2), 2)
--   ))) < 5000
CREATE TABLE IF NOT EXISTS `waste_reports` (
    `id`                CHAR(36)      NOT NULL DEFAULT (UUID()),
    `citizen_id`        CHAR(36)      NOT NULL,
    `waste_category_id` INT           NULL,
    `description`       TEXT          NULL,
    `latitude`          DECIMAL(10,8) NOT NULL,   -- replaces geography(Point)
    `longitude`         DECIMAL(11,8) NOT NULL,   -- replaces geography(Point)
    `address`           VARCHAR(500)  NULL,
    `status`            ENUM('pending','accepted','assigned','collected','rejected')
                                      NOT NULL DEFAULT 'pending',
    `ai_suggestion`     VARCHAR(50)   NULL,
    `created_at`        DATETIME(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_reports_citizen`
        FOREIGN KEY (`citizen_id`) REFERENCES `users`(`id`) ON DELETE RESTRICT,
    CONSTRAINT `fk_reports_category`
        FOREIGN KEY (`waste_category_id`) REFERENCES `waste_categories`(`id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 7. report_images
CREATE TABLE IF NOT EXISTS `report_images` (
    `id`         CHAR(36)     NOT NULL DEFAULT (UUID()),
    `report_id`  CHAR(36)     NOT NULL,
    `image_url`  VARCHAR(500) NOT NULL,
    `sort_order` INT          NOT NULL DEFAULT 0,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_report_images_report`
        FOREIGN KEY (`report_id`) REFERENCES `waste_reports`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 8. collection_tasks
CREATE TABLE IF NOT EXISTS `collection_tasks` (
    `id`                  CHAR(36)       NOT NULL DEFAULT (UUID()),
    `report_id`           CHAR(36)       NOT NULL,
    `enterprise_id`       CHAR(36)       NOT NULL,
    `collector_id`        CHAR(36)       NULL,
    `status`              ENUM('assigned','on_the_way','collected')
                                         NOT NULL DEFAULT 'assigned',
    `collected_weight_kg` DECIMAL(8,2)   NULL,
    `notes`               TEXT           NULL,
    `assigned_at`         DATETIME(6)    NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `completed_at`        DATETIME(6)    NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_tasks_report_id` (`report_id`),
    CONSTRAINT `fk_tasks_report`
        FOREIGN KEY (`report_id`) REFERENCES `waste_reports`(`id`),
    CONSTRAINT `fk_tasks_enterprise`
        FOREIGN KEY (`enterprise_id`) REFERENCES `enterprises`(`id`),
    CONSTRAINT `fk_tasks_collector`
        FOREIGN KEY (`collector_id`) REFERENCES `collectors`(`id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 9. task_status_logs
CREATE TABLE IF NOT EXISTS `task_status_logs` (
    `id`         CHAR(36)    NOT NULL DEFAULT (UUID()),
    `task_id`    CHAR(36)    NOT NULL,
    `status`     ENUM('assigned','on_the_way','collected') NOT NULL,
    `changed_at` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_status_logs_task`
        FOREIGN KEY (`task_id`) REFERENCES `collection_tasks`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 10. collection_images
CREATE TABLE IF NOT EXISTS `collection_images` (
    `id`        CHAR(36)     NOT NULL DEFAULT (UUID()),
    `task_id`   CHAR(36)     NOT NULL,
    `image_url` VARCHAR(500) NOT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_collection_images_task`
        FOREIGN KEY (`task_id`) REFERENCES `collection_tasks`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 11. reward_rules
CREATE TABLE IF NOT EXISTS `reward_rules` (
    `id`                CHAR(36)   NOT NULL DEFAULT (UUID()),
    `enterprise_id`     CHAR(36)   NOT NULL,
    `waste_category_id` INT        NOT NULL,
    `points_per_report` INT        NOT NULL DEFAULT 10,
    `bonus_quality`     INT        NOT NULL DEFAULT 0,
    `is_active`         TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_reward_rules` (`enterprise_id`, `waste_category_id`),
    CONSTRAINT `fk_reward_rules_enterprise`
        FOREIGN KEY (`enterprise_id`) REFERENCES `enterprises`(`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_reward_rules_category`
        FOREIGN KEY (`waste_category_id`) REFERENCES `waste_categories`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 12. reward_points
CREATE TABLE IF NOT EXISTS `reward_points` (
    `id`               CHAR(36)     NOT NULL DEFAULT (UUID()),
    `citizen_id`       CHAR(36)     NOT NULL,
    `report_id`        CHAR(36)     NULL,
    `idempotency_key`  VARCHAR(100) NULL,          -- prevent double-reward
    `points`           INT          NOT NULL,
    `reason`           VARCHAR(255) NULL,
    `created_at`       DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_reward_idempotency` (`idempotency_key`),
    CONSTRAINT `fk_reward_points_citizen`
        FOREIGN KEY (`citizen_id`) REFERENCES `users`(`id`),
    CONSTRAINT `fk_reward_points_report`
        FOREIGN KEY (`report_id`) REFERENCES `waste_reports`(`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 13. complaints
CREATE TABLE IF NOT EXISTS `complaints` (
    `id`             CHAR(36)    NOT NULL DEFAULT (UUID()),
    `citizen_id`     CHAR(36)    NOT NULL,
    `report_id`      CHAR(36)    NULL,
    `content`        TEXT        NOT NULL,
    `status`         ENUM('open','in_progress','resolved','rejected')
                                 NOT NULL DEFAULT 'open',
    `admin_response` TEXT        NULL,
    `created_at`     DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `resolved_at`    DATETIME(6) NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_complaints_citizen`
        FOREIGN KEY (`citizen_id`) REFERENCES `users`(`id`),
    CONSTRAINT `fk_complaints_report`
        FOREIGN KEY (`report_id`) REFERENCES `waste_reports`(`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 14. audit_logs
CREATE TABLE IF NOT EXISTS `audit_logs` (
    `id`          CHAR(36)     NOT NULL DEFAULT (UUID()),
    `user_id`     CHAR(36)     NULL,
    `action`      VARCHAR(100) NOT NULL,
    `entity_type` VARCHAR(50)  NULL,
    `entity_id`   CHAR(36)     NULL,
    `ip_address`  VARCHAR(45)  NULL,     -- replaces INET type (supports IPv6)
    `created_at`  DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_audit_logs_user`
        FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

COMMIT;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- INDEXES (run separately after tables are created)
-- ============================================================

-- users
CREATE INDEX idx_users_role          ON `users`(`role`);
CREATE INDEX idx_users_district_ward ON `users`(`district`, `ward`);

-- waste_reports
-- ⚠️ No GiST index in MySQL. Use lat/lon B-TREE index for bounding-box pre-filter.
CREATE INDEX idx_reports_lat_lon   ON `waste_reports`(`latitude`, `longitude`);
CREATE INDEX idx_reports_citizen   ON `waste_reports`(`citizen_id`);
CREATE INDEX idx_reports_status    ON `waste_reports`(`status`);
CREATE INDEX idx_reports_category  ON `waste_reports`(`waste_category_id`);
CREATE INDEX idx_reports_created   ON `waste_reports`(`created_at` DESC);

-- collection_tasks
CREATE INDEX idx_tasks_enterprise  ON `collection_tasks`(`enterprise_id`);
CREATE INDEX idx_tasks_collector   ON `collection_tasks`(`collector_id`);
CREATE INDEX idx_tasks_status      ON `collection_tasks`(`status`);

-- task_status_logs
CREATE INDEX idx_status_logs_task  ON `task_status_logs`(`task_id`);

-- reward_points
CREATE INDEX idx_rewards_citizen   ON `reward_points`(`citizen_id`, `created_at` DESC);

-- complaints
CREATE INDEX idx_complaints_citizen ON `complaints`(`citizen_id`);
CREATE INDEX idx_complaints_status  ON `complaints`(`status`);

-- audit_logs
CREATE INDEX idx_audit_user         ON `audit_logs`(`user_id`, `created_at` DESC);
CREATE INDEX idx_audit_entity       ON `audit_logs`(`entity_type`, `entity_id`);

-- ============================================================
-- STATE MACHINE TRIGGERS (MySQL DELIMITER syntax)
-- NOTE: Run each DELIMITER block separately in MySQL CLI or Workbench
-- ============================================================

DELIMITER //

-- Trigger: Validate waste_report status transitions
CREATE TRIGGER trg_validate_report_status
BEFORE UPDATE ON `waste_reports`
FOR EACH ROW
BEGIN
    -- Only validate when status changes
    IF OLD.status <> NEW.status THEN
        -- pending → accepted | rejected  (valid)
        IF OLD.status = 'pending' AND NEW.status NOT IN ('accepted', 'rejected') THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Invalid status transition: pending can only go to accepted or rejected';
        END IF;
        -- accepted → assigned  (valid)
        IF OLD.status = 'accepted' AND NEW.status <> 'assigned' THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Invalid status transition: accepted can only go to assigned';
        END IF;
        -- assigned → on_the_way  (valid)
        IF OLD.status = 'assigned' AND NEW.status <> 'on_the_way' THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Invalid status transition: assigned can only go to on_the_way';
        END IF;
        -- on_the_way → collected  (valid)
        IF OLD.status = 'on_the_way' AND NEW.status <> 'collected' THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Invalid status transition: on_the_way can only go to collected';
        END IF;
        -- collected | rejected → no further transitions
        IF OLD.status IN ('collected', 'rejected') THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Invalid status transition: terminal status cannot change';
        END IF;
    END IF;
END //

-- Trigger: Validate collection_task status transitions
CREATE TRIGGER trg_validate_task_status
BEFORE UPDATE ON `collection_tasks`
FOR EACH ROW
BEGIN
    IF OLD.status <> NEW.status THEN
        IF OLD.status = 'assigned' AND NEW.status <> 'on_the_way' THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Task: assigned can only go to on_the_way';
        END IF;
        IF OLD.status = 'on_the_way' AND NEW.status <> 'collected' THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Task: on_the_way can only go to collected';
        END IF;
        IF OLD.status = 'collected' THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Task: collected is a terminal status';
        END IF;
    END IF;
END //

DELIMITER ;

-- ============================================================
-- LEADERBOARD VIEW
-- ⚠️ MySQL has NO Materialized Views. Use VIEW instead.
-- For performance, refresh cached data via application-layer cron (every 5 min).
-- ============================================================

CREATE OR REPLACE VIEW `v_leaderboard` AS
SELECT
    rp.citizen_id,
    u.full_name,
    u.district,
    u.ward,
    SUM(rp.points)                                          AS total_points,
    RANK() OVER (ORDER BY SUM(rp.points) DESC)             AS `rank`,
    RANK() OVER (PARTITION BY u.district ORDER BY SUM(rp.points) DESC) AS district_rank
FROM `reward_points` rp
JOIN `users` u ON u.id = rp.citizen_id
GROUP BY rp.citizen_id, u.full_name, u.district, u.ward;

-- ============================================================
-- HAVERSINE SPATIAL QUERY HELPER (comment reference)
-- Use in application layer or stored procedure:
-- Find reports within 5km of a given point (lat, lon):
-- ============================================================
-- SELECT id, latitude, longitude, address, status,
--   (6371000 * 2 * ASIN(SQRT(
--     POW(SIN(RADIANS((latitude - :lat) / 2)), 2) +
--     COS(RADIANS(:lat)) * COS(RADIANS(latitude)) *
--     POW(SIN(RADIANS((longitude - :lon) / 2)), 2)
--   ))) AS distance_meters
-- FROM waste_reports
-- WHERE
--   latitude  BETWEEN :lat - 0.045 AND :lat + 0.045   -- bounding box pre-filter
--   AND longitude BETWEEN :lon - 0.045 AND :lon + 0.045
--   AND status = 'pending'
-- HAVING distance_meters < 5000
-- ORDER BY distance_meters;

-- ============================================================
-- ROLLBACK SCRIPT (run in emergency)
-- ============================================================
-- SET FOREIGN_KEY_CHECKS = 0;
-- DROP VIEW  IF EXISTS v_leaderboard;
-- DROP TRIGGER IF EXISTS trg_validate_report_status;
-- DROP TRIGGER IF EXISTS trg_validate_task_status;
-- DROP TABLE IF EXISTS audit_logs, complaints, reward_points,
--   reward_rules, collection_images, task_status_logs,
--   collection_tasks, report_images, waste_reports,
--   enterprise_waste_types, collectors, waste_categories,
--   enterprises, users;
-- SET FOREIGN_KEY_CHECKS = 1;
