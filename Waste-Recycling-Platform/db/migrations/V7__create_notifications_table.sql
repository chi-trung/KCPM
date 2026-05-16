-- Migration: Create Notifications Table for Citizen Notification System
-- Date: 2026-04-17

CREATE TABLE IF NOT EXISTS notifications (
    id CHAR(36) PRIMARY KEY,
    citizen_id CHAR(36) NULL,                               -- NULL nếu là thông báo chung cho admin
    type VARCHAR(50) NOT NULL,
    channel VARCHAR(20) NOT NULL DEFAULT 'InApp',
    status VARCHAR(20) NOT NULL DEFAULT 'Unread',
    title VARCHAR(200) NOT NULL,
    message VARCHAR(1000) NOT NULL,
    action_url VARCHAR(500),
    related_entity_id CHAR(36),
    related_entity_type VARCHAR(50),
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    read_at DATETIME(6),
    
    FOREIGN KEY (citizen_id) REFERENCES users(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Indexes for performance
CREATE INDEX idx_notifications_citizen_id ON notifications(citizen_id);
CREATE INDEX idx_notifications_citizen_status ON notifications(citizen_id, status);
CREATE INDEX idx_notifications_created_at ON notifications(created_at);

-- Add comment to table
ALTER TABLE notifications COMMENT = 'Stores notifications for citizens triggered by system events';
