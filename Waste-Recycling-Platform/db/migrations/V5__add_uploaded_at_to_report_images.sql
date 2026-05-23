-- Add uploaded_at column to report_images table
ALTER TABLE report_images ADD COLUMN uploaded_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6);
