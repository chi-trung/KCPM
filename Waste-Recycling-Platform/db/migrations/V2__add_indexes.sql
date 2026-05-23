-- ============================================================
-- V2: Indexes
-- MySQL 8.0+ | Waste Collection & Recycling Platform
-- ============================================================

-- users
CREATE INDEX idx_users_role          ON `users`(`role`);
CREATE INDEX idx_users_district_ward ON `users`(`district`, `ward`);

-- waste_reports (lat/lon B-TREE pre-filter, then Haversine in app layer)
CREATE INDEX idx_reports_lat_lon    ON `waste_reports`(`latitude`, `longitude`);
CREATE INDEX idx_reports_citizen    ON `waste_reports`(`citizen_id`);
CREATE INDEX idx_reports_status     ON `waste_reports`(`status`);
CREATE INDEX idx_reports_category   ON `waste_reports`(`waste_category_id`);
CREATE INDEX idx_reports_created    ON `waste_reports`(`created_at` DESC);

-- collection_tasks
CREATE INDEX idx_tasks_enterprise   ON `collection_tasks`(`enterprise_id`);
CREATE INDEX idx_tasks_collector    ON `collection_tasks`(`collector_id`);
CREATE INDEX idx_tasks_status       ON `collection_tasks`(`status`);

-- task_status_logs
CREATE INDEX idx_status_logs_task   ON `task_status_logs`(`task_id`);

-- reward_points
CREATE INDEX idx_rewards_citizen    ON `reward_points`(`citizen_id`, `created_at` DESC);

-- complaints
CREATE INDEX idx_complaints_citizen   ON `complaints`(`citizen_id`);
CREATE INDEX idx_complaints_collector ON `complaints`(`collector_id`);
CREATE INDEX idx_complaints_enterprise ON `complaints`(`enterprise_id`);
CREATE INDEX idx_complaints_status    ON `complaints`(`status`);

-- audit_logs
CREATE INDEX idx_audit_user         ON `audit_logs`(`user_id`, `created_at` DESC);
CREATE INDEX idx_audit_entity       ON `audit_logs`(`entity_type`, `entity_id`);
