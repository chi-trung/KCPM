SET FOREIGN_KEY_CHECKS=0;

-- Drop existing FK (expected name from current schema)
ALTER TABLE notifications DROP FOREIGN KEY `fk_notifications_user`;

-- Rename user_id -> citizen_id (allow NULL)
ALTER TABLE notifications CHANGE COLUMN `user_id` `citizen_id` CHAR(36) COLLATE utf8mb4_unicode_ci NULL;

-- Resize text columns
ALTER TABLE notifications MODIFY COLUMN `title` VARCHAR(200) COLLATE utf8mb4_unicode_ci NOT NULL;
ALTER TABLE notifications MODIFY COLUMN `message` VARCHAR(1000) COLLATE utf8mb4_unicode_ci NOT NULL;

-- Add new columns
ALTER TABLE notifications ADD COLUMN `channel` VARCHAR(20) NOT NULL DEFAULT 'InApp' AFTER `type`;
ALTER TABLE notifications ADD COLUMN `status` VARCHAR(20) NOT NULL DEFAULT 'Unread' AFTER `channel`;
ALTER TABLE notifications ADD COLUMN `action_url` VARCHAR(500) NULL AFTER `message`;
ALTER TABLE notifications ADD COLUMN `related_entity_type` VARCHAR(50) NULL AFTER `related_entity_id`;
ALTER TABLE notifications ADD COLUMN `read_at` DATETIME(6) NULL AFTER `created_at`;

-- Migrate data from legacy is_read
UPDATE notifications SET `status` = CASE WHEN `is_read`=1 THEN 'Read' ELSE 'Unread' END;
UPDATE notifications SET `read_at` = `created_at` WHERE `is_read`=1 AND `read_at` IS NULL;

-- Drop legacy column
ALTER TABLE notifications DROP COLUMN `is_read`;

-- Add new FK
ALTER TABLE notifications ADD CONSTRAINT `fk_notifications_citizen` FOREIGN KEY (`citizen_id`) REFERENCES `users`(`id`) ON DELETE SET NULL;

-- Indexes
CREATE INDEX `idx_notifications_citizen_id` ON notifications(`citizen_id`);
CREATE INDEX `idx_notifications_citizen_status` ON notifications(`citizen_id`,`status`);
CREATE INDEX `idx_notifications_created_at` ON notifications(`created_at`);

ALTER TABLE notifications COMMENT = 'Stores notifications for citizens triggered by system events';

SET FOREIGN_KEY_CHECKS=1;
