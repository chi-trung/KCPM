-- ============================================================
-- V1: Create all base tables
-- MySQL 8.0+ | Waste Collection & Recycling Platform
-- ============================================================
SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

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
    `updated_at`    DATETIME(6)   NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_users_email` (`email`),
    UNIQUE KEY `uq_users_phone` (`phone`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `enterprises` (
    `id`                  CHAR(36)     NOT NULL DEFAULT (UUID()),
    `user_id`             CHAR(36)     NOT NULL,
    `company_name`        VARCHAR(200) NOT NULL,
    `service_area`        JSON         NULL,
    `capacity_kg_per_day` INT          NULL,
    `is_verified`         TINYINT(1)   NOT NULL DEFAULT 0,
    `created_at`          DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_enterprises_user_id` (`user_id`),
    CONSTRAINT `fk_enterprises_user` FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `waste_categories` (
    `id`          INT         NOT NULL AUTO_INCREMENT,
    `name`        VARCHAR(50) NOT NULL,
    `description` TEXT        NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_waste_categories_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `collectors` (
    `id`            CHAR(36)   NOT NULL DEFAULT (UUID()),
    `user_id`       CHAR(36)   NOT NULL,
    `enterprise_id` CHAR(36)   NOT NULL,
    `is_available`  TINYINT(1) NOT NULL DEFAULT 1,
    `created_at`    DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_collectors_user_id` (`user_id`),
    CONSTRAINT `fk_collectors_user`       FOREIGN KEY (`user_id`)       REFERENCES `users`(`id`)        ON DELETE CASCADE,
    CONSTRAINT `fk_collectors_enterprise` FOREIGN KEY (`enterprise_id`) REFERENCES `enterprises`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `enterprise_waste_types` (
    `id`                CHAR(36) NOT NULL DEFAULT (UUID()),
    `enterprise_id`     CHAR(36) NOT NULL,
    `waste_category_id` INT      NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_enterprise_waste_type` (`enterprise_id`, `waste_category_id`),
    CONSTRAINT `fk_ewt_enterprise` FOREIGN KEY (`enterprise_id`)     REFERENCES `enterprises`(`id`)     ON DELETE CASCADE,
    CONSTRAINT `fk_ewt_category`   FOREIGN KEY (`waste_category_id`) REFERENCES `waste_categories`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- GPS stored as DECIMAL (PostGIS not available in MySQL)
-- Use Haversine formula for radius queries — see project_structure.md
CREATE TABLE IF NOT EXISTS `waste_reports` (
    `id`                CHAR(36)      NOT NULL DEFAULT (UUID()),
    `citizen_id`        CHAR(36)      NOT NULL,
    `waste_category_id` INT           NULL,
    `description`       TEXT          NULL,
    `latitude`          DECIMAL(10,8) NOT NULL,
    `longitude`         DECIMAL(11,8) NOT NULL,
    `address`           VARCHAR(500)  NULL,
    `status`            ENUM('pending','accepted','assigned','collected','rejected') NOT NULL DEFAULT 'pending',
    `ai_suggestion`     VARCHAR(50)   NULL,
    `created_at`        DATETIME(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_reports_citizen`  FOREIGN KEY (`citizen_id`)        REFERENCES `users`(`id`)            ON DELETE RESTRICT,
    CONSTRAINT `fk_reports_category` FOREIGN KEY (`waste_category_id`) REFERENCES `waste_categories`(`id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `report_images` (
    `id`         CHAR(36)     NOT NULL DEFAULT (UUID()),
    `report_id`  CHAR(36)     NOT NULL,
    `image_url`  VARCHAR(500) NOT NULL,
    `sort_order` INT          NOT NULL DEFAULT 0,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_report_images_report` FOREIGN KEY (`report_id`) REFERENCES `waste_reports`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `collection_tasks` (
    `id`                  CHAR(36)     NOT NULL DEFAULT (UUID()),
    `report_id`           CHAR(36)     NOT NULL,
    `enterprise_id`       CHAR(36)     NOT NULL,
    `collector_id`        CHAR(36)     NULL,
    `status`              ENUM('assigned','on_the_way','collected') NOT NULL DEFAULT 'assigned',
    `collected_weight_kg` DECIMAL(8,2) NULL,
    `notes`               TEXT         NULL,
    `assigned_at`         DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `completed_at`        DATETIME(6)  NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_tasks_report_id` (`report_id`),
    CONSTRAINT `fk_tasks_report`      FOREIGN KEY (`report_id`)     REFERENCES `waste_reports`(`id`),
    CONSTRAINT `fk_tasks_enterprise`  FOREIGN KEY (`enterprise_id`) REFERENCES `enterprises`(`id`),
    CONSTRAINT `fk_tasks_collector`   FOREIGN KEY (`collector_id`)  REFERENCES `collectors`(`id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `task_status_logs` (
    `id`         CHAR(36)    NOT NULL DEFAULT (UUID()),
    `task_id`    CHAR(36)    NOT NULL,
    `status`     ENUM('assigned','on_the_way','collected') NOT NULL,
    `changed_at` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_status_logs_task` FOREIGN KEY (`task_id`) REFERENCES `collection_tasks`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `collection_images` (
    `id`        CHAR(36)     NOT NULL DEFAULT (UUID()),
    `task_id`   CHAR(36)     NOT NULL,
    `image_url` VARCHAR(500) NOT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_collection_images_task` FOREIGN KEY (`task_id`) REFERENCES `collection_tasks`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `reward_rules` (
    `id`                CHAR(36)   NOT NULL DEFAULT (UUID()),
    `enterprise_id`     CHAR(36)   NOT NULL,
    `waste_category_id` INT        NOT NULL,
    `points_per_report` INT        NOT NULL DEFAULT 10,
    `bonus_quality`     INT        NOT NULL DEFAULT 0,
    `is_active`         TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_reward_rules` (`enterprise_id`, `waste_category_id`),
    CONSTRAINT `fk_reward_rules_enterprise` FOREIGN KEY (`enterprise_id`)     REFERENCES `enterprises`(`id`)     ON DELETE CASCADE,
    CONSTRAINT `fk_reward_rules_category`   FOREIGN KEY (`waste_category_id`) REFERENCES `waste_categories`(`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `reward_points` (
    `id`               CHAR(36)     NOT NULL DEFAULT (UUID()),
    `citizen_id`       CHAR(36)     NOT NULL,
    `report_id`        CHAR(36)     NULL,
    `idempotency_key`  VARCHAR(100) NULL,
    `points`           INT          NOT NULL,
    `reason`           VARCHAR(255) NULL,
    `created_at`       DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`id`),
    UNIQUE KEY `uq_reward_idempotency` (`idempotency_key`),
    CONSTRAINT `fk_reward_points_citizen` FOREIGN KEY (`citizen_id`) REFERENCES `users`(`id`),
    CONSTRAINT `fk_reward_points_report`  FOREIGN KEY (`report_id`)  REFERENCES `waste_reports`(`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `complaints` (
    `id`                      CHAR(36)    NOT NULL DEFAULT (UUID()),
    `citizen_id`              CHAR(36)    NOT NULL,
    `report_id`               CHAR(36)    NULL,
    `collector_id`            CHAR(36)    NULL,
    `enterprise_id`           CHAR(36)    NULL,
    `content`                 TEXT        NOT NULL,
    `status`                  ENUM('open','in_progress','resolved','rejected','escalated') NOT NULL DEFAULT 'open',
    `admin_response`          TEXT        NULL,
    `enterprise_response`     VARCHAR(2000) NULL,
    `enterprise_responded_at` DATETIME(6) NULL,
    `escalation_reason`       VARCHAR(1000) NULL,
    `created_at`              DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at`              DATETIME(6) NULL,
    `resolved_at`             DATETIME(6) NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_complaints_citizen` FOREIGN KEY (`citizen_id`) REFERENCES `users`(`id`),
    CONSTRAINT `fk_complaints_report`  FOREIGN KEY (`report_id`)  REFERENCES `waste_reports`(`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `audit_logs` (
    `id`          CHAR(36)     NOT NULL DEFAULT (UUID()),
    `user_id`     CHAR(36)     NULL,
    `action`      VARCHAR(100) NOT NULL,
    `entity_type` VARCHAR(50)  NULL,
    `entity_id`   CHAR(36)     NULL,
    `ip_address`  VARCHAR(45)  NULL,
    `created_at`  DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_audit_logs_user` FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SET FOREIGN_KEY_CHECKS = 1;
