-- ============================================================
-- V7: Add Status and RejectionReason fields to enterprises
-- ============================================================

ALTER TABLE `enterprises` 
ADD COLUMN `status` ENUM('Pending', 'Verified', 'Rejected') NOT NULL DEFAULT 'Pending' AFTER `is_verified`,
ADD COLUMN `rejection_reason` VARCHAR(500) NULL AFTER `status`;

-- For existing verified enterprises, update status to 'Verified'
UPDATE `enterprises` SET `status` = 'Verified' WHERE `is_verified` = 1;

-- For existing non-verified enterprises, keep status as 'Pending' (already the default)
